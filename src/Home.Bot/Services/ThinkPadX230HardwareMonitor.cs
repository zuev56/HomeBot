using Home.Bot.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Scheduler;

namespace Home.Bot.Services
{
    public class ThinkPadX230HardwareMonitor : IHardwareMonitor
    {
        // TODO: receive commands and send answers
        // TODO: Analyse sensors, memory, CPU loading in jobs
        // TODO: Information about bad battery!!!
        private readonly IConfiguration _configuration;
        private readonly IMessenger _messenger;
        private readonly IShellLauncher _shellLauncher;
        private readonly ILogger<ThinkPadX230HardwareMonitor> _logger;

        public IReadOnlyCollection<IJobBase> Jobs { get; init; }

        public ThinkPadX230HardwareMonitor(
            IConfiguration configuration,
            IMessenger messenger,
            IShellLauncher shellLauncher,
            ILogger<ThinkPadX230HardwareMonitor> logger)
        {
            Jobs = new List<IJob>();
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _shellLauncher = shellLauncher ?? throw new ArgumentNullException(nameof(shellLauncher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Jobs = CreateJobs();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInformation($"{nameof(ThinkPadX230HardwareMonitor)} started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInformation($"{nameof(ThinkPadX230HardwareMonitor)} stopped");
            return Task.CompletedTask;
        }

        private List<IJobBase> CreateJobs()
        {
            var hardwareMonitorJob = new ProgramJob<string>(
                period: TimeSpan.FromMinutes(5),
                method: CombineHardwareAnalyzeResults,
                startUtcDate: DateTime.UtcNow + TimeSpan.FromSeconds(5),
                description: Constants.HARDWARE_MONITOR_INFORMING_JOB_NAME
            );
           
            return new List<IJobBase>() { hardwareMonitorJob };
        }

        private async Task<string> CombineHardwareAnalyzeResults()
        {
            var fullAnalyzeResult = await AnalyzeCpuTemperature();
            fullAnalyzeResult.Merge(await AnalyzeMemoryUsage());
            fullAnalyzeResult.Merge(await AnalyzeCpuUsage());

            return fullAnalyzeResult.JoinMessages();
        }

        public async Task<float> GetCpuTemperature()
        {
            var commandResult = await _shellLauncher.RunBashAsync("sensors -j");

            if (!commandResult.IsSuccess)
                throw new Exception(commandResult.Messages.Single().Text);

            if (string.IsNullOrWhiteSpace(commandResult.Value))
                throw new Exception("Empty result");

            _logger.LogTrace("Bash command result: {Result}", commandResult.ToJSON());

            var jsonNode = JsonNode.Parse(commandResult.Value);

            return jsonNode["coretemp-isa-0000"]["Package id 0"]["temp1_input"].GetValue<float>();
        }

        public async Task<double> GetMemoryUsagePercent()
        {
            var commandResult = await _shellLauncher.RunBashAsync("egrep 'Mem|Cache|Swap' /proc/meminfo");
            // Approximate result:
            // MemTotal:       16067104 kB
            // MemAvailable:   12935852 kB
            // ...

            if (!commandResult.IsSuccess)
                throw new Exception(commandResult.Messages.Single().Text);

            if (string.IsNullOrWhiteSpace(commandResult.Value))
                throw new Exception("Empty result");

            _logger.LogTrace("Bash command result: {Result}", commandResult.ToJSON());

            var memUsage = commandResult.Value
                    .Split("kB", StringSplitOptions.RemoveEmptyEntries)
                    .Where(row => !string.IsNullOrWhiteSpace(row.Trim()))
                    .Select(row => {
                        var cells = row.Split(':');
                        return new
                        {
                            Name = cells[0].Trim(),
                            Size = int.Parse(cells[1].Trim())
                        };
                    })
                    .ToArray();

            int total = memUsage.Single(i => i.Name == Constants.MEM_INFO_TOTAL).Size;
            int availavle = memUsage.Single(i => i.Name == Constants.MEM_INFO_AVAILABLE).Size;

            return 100 - availavle / (double)total * 100;
        }

        public async Task<float> GetCpuUsage() => (await HtopCpuUsage())[0];

        public async Task<float> Get5minAvgCpuUsage() => (await HtopCpuUsage())[1];

        public async Task<float> Get15minAvgCpuUsage() => (await HtopCpuUsage())[2];

        private async Task<float[]> HtopCpuUsage()
        {
            var commandResult = await _shellLauncher.RunBashAsync("cat /proc/loadavg | awk '{print $1\"-\"$2\"-\"$3}'");
            // Approximate result: 0.07-0.06-0.01

            if (!commandResult.IsSuccess)
                throw new Exception(commandResult.Messages.Single().Text);

            if (string.IsNullOrWhiteSpace(commandResult.Value))
                throw new Exception("Empty result");

            _logger.LogTrace("Bash command result: {Result}", commandResult.ToJSON());

            return commandResult.Value
                .Split('-')
                .Select(i => float.Parse(i, CultureInfo.InvariantCulture))
                .ToArray();
        }

        private async Task<IOperationResult> AnalyzeCpuTemperature()
        {
            try
            {
                var cpuTemperature = await GetCpuTemperature();

                _logger.LogDebug("CPU temperature: {CPUTemperature}°C", cpuTemperature.ToString("0.##"));

                return cpuTemperature >= _configuration.GetSection("Home:ComputerManager:WarnTemperature").Get<float>()
                    ? ServiceResult.Warning($"CPU temperature: {cpuTemperature}°C")
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogErrorIfNeed(ex, "Unable to get temperature sensors info: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
                return ServiceResult.Error($"Unable to get temperature sensors info: {ex.Message}");
            }
        }

        private async Task<IOperationResult> AnalyzeMemoryUsage()
        {
            try
            {
                double memoryUsagePercent = await GetMemoryUsagePercent();

                _logger.LogDebug("Memory usage {MemoryUsage}%", Math.Round(memoryUsagePercent, 0));

                return memoryUsagePercent > _configuration.GetSection("Home:ComputerManager:WarnMemoryUsagePercent").Get<int>()
                    ? ServiceResult.Warning($"Memory usage {memoryUsagePercent}%")
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogErrorIfNeed(ex, "Unable to get memory usage: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
                return ServiceResult.Error($"Unable to get memory usage: {ex.Message}");
            }
        }

        private async Task<IOperationResult> AnalyzeCpuUsage()
        {
            try
            {
                var cpuUsage = await Get15minAvgCpuUsage();

                _logger.LogDebug("CPU usage {CpuUsage}", cpuUsage);

                return cpuUsage > _configuration.GetSection("Home:ComputerManager:WarnCpuUsage").Get<float>()
                    ? ServiceResult.Warning($"15 minutes average CPU usage {cpuUsage}")
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogErrorIfNeed(ex, "Unable to get CPU usage: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
                return ServiceResult.Error($"Unable to get CPU usage: {ex.Message}");
            }
        }

    }
}
