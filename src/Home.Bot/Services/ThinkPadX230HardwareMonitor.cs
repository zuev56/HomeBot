using System;
using System.Collections.Generic;
using Home.Bot.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Services.Messaging;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Scheduling;
using Zs.Common.Services.Shell;

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

        public IReadOnlyCollection<JobBase> Jobs { get; init; }

        public ThinkPadX230HardwareMonitor(
            IConfiguration configuration,
            IMessenger messenger,
            IShellLauncher shellLauncher,
            ILogger<ThinkPadX230HardwareMonitor> logger)
        {
            Jobs = new List<Job>();
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

        private List<JobBase> CreateJobs()
        {
            var hardwareMonitorJob = new ProgramJob<string>(
                period: TimeSpan.FromMinutes(5),
                method: CombineHardwareAnalyzeResults,
                startUtcDate: DateTime.UtcNow + TimeSpan.FromSeconds(5),
                description: Constants.HardwareWarningsInformer
            );

            return new List<JobBase> { hardwareMonitorJob };
        }

        private async Task<string> CombineHardwareAnalyzeResults()
        {
            // TODO: Make parallel
            var cpuTemperatureResult = await AnalyzeCpuTemperature();
            var memoryUsageResult = await AnalyzeMemoryUsage();
            var cpuUsageResult = await AnalyzeCpuUsage();

            var message = (cpuTemperatureResult.Successful ? cpuTemperatureResult.Value : cpuTemperatureResult.Fault!.Message) + Environment.NewLine + Environment.NewLine
                + (memoryUsageResult.Successful ? memoryUsageResult.Value : memoryUsageResult.Fault!.Message) + Environment.NewLine + Environment.NewLine
                + (cpuUsageResult.Successful ? cpuUsageResult.Value : cpuUsageResult.Fault!.Message);

            return message;
        }

        public async Task<float> GetCpuTemperature()
        {
            var commandResult = await _shellLauncher.RunBashAsync("sensors -j");

            if (!commandResult.Successful)
            {
                throw new FaultException(commandResult.Fault!);
            }

            if (string.IsNullOrWhiteSpace(commandResult.Value))
            {
                throw new FaultException(Fault.Unknown.SetMessage("Empty result"));
            }

            //_logger.LogTrace("Bash command result: {Result}", commandResult.ToJSON());

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

            if (!commandResult.Successful)
            {
                throw new FaultException(commandResult.Fault!);
            }

            if (string.IsNullOrWhiteSpace(commandResult.Value))
            {
                throw new FaultException(Fault.Unknown.SetMessage("Empty result"));
            }

            //_logger.LogTrace("Bash command result: {Result}", commandResult.ToJSON());

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

            var total = memUsage.Single(i => i.Name == Constants.MemTotal).Size;
            var available = memUsage.Single(i => i.Name == Constants.MemAvailable).Size;

            return 100 - available / (double)total * 100;
        }

        public async Task<float> GetCpuUsage() => (await HtopCpuUsage())[0];

        public async Task<float> Get5minAvgCpuUsage() => (await HtopCpuUsage())[1];

        public async Task<float> Get15minAvgCpuUsage() => (await HtopCpuUsage())[2];

        private async Task<float[]> HtopCpuUsage()
        {
            var commandResult = await _shellLauncher.RunBashAsync("cat /proc/loadavg | awk '{print $1\"-\"$2\"-\"$3}'");
            // Approximate result: 0.07-0.06-0.01

            if (!commandResult.Successful)
            {
                throw new FaultException(commandResult.Fault!);
            }

            if (string.IsNullOrWhiteSpace(commandResult.Value))
            {
                throw new FaultException(Fault.Unknown.SetMessage("Empty result"));
            }

            //_logger.LogTrace("Bash command result: {Result}", commandResult.ToJSON());

            return commandResult.Value
                .Split('-')
                .Select(i => float.Parse(i, CultureInfo.InvariantCulture))
                .ToArray();
        }

        private async Task<Result<string>> AnalyzeCpuTemperature()
        {
            try
            {
                var cpuTemperature = await GetCpuTemperature();

                _logger.LogDebug("CPU temperature: {CPUTemperature}°C", cpuTemperature.ToString("0.##"));

                return cpuTemperature >= _configuration.GetSection("Home:ComputerManager:WarnTemperature").Get<float>()
                    ? Result.Success($"CPU temperature: {cpuTemperature}°C")
                    : Result.Success(string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogErrorIfNeed(ex, "Unable to get temperature sensors info: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
                return Result.Fail<string>(Fault.Unknown.SetMessage($"Unable to get temperature sensors info: {ex.Message}"));
            }
        }

        private async Task<Result<string>> AnalyzeMemoryUsage()
        {
            try
            {
                double memoryUsagePercent = await GetMemoryUsagePercent();

                _logger.LogDebug("Memory usage {MemoryUsage}%", Math.Round(memoryUsagePercent, 0));

                return memoryUsagePercent > _configuration.GetSection("Home:ComputerManager:WarnMemoryUsagePercent").Get<int>()
                    ? Result.Success($"Memory usage {memoryUsagePercent}%")
                    : Result.Success(string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogErrorIfNeed(ex, "Unable to get memory usage: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
                return Result.Fail<string>(Fault.Unknown.SetMessage($"Unable to get memory usage: {ex.Message}"));
            }
        }

        private async Task<Result<string>> AnalyzeCpuUsage()
        {
            try
            {
                var cpuUsage = await Get15minAvgCpuUsage();

                _logger.LogDebug("CPU usage {CpuUsage}", cpuUsage);

                return cpuUsage > _configuration.GetSection("Home:ComputerManager:WarnCpuUsage").Get<float>()
                    ? Result.Success($"15 minutes average CPU usage {cpuUsage}")
                    : Result.Success(string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogErrorIfNeed(ex, "Unable to get CPU usage: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
                return Result.Fail<string>(Fault.Unknown.SetMessage($"Unable to get CPU usage: {ex.Message}"));
            }
        }

    }
}