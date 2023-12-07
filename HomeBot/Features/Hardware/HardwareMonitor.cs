using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Common.Services.Scheduling;
using static System.Environment;

namespace HomeBot.Features.Hardware;

public abstract class HardwareMonitor : IHasJob, IHasCurrentState
{
    protected readonly HardwareMonitorSettings Options;
    protected readonly ILogger<HardwareMonitor> Logger;

    public ProgramJob<string> Job { get; }

    protected HardwareMonitor(
        IOptions<HardwareMonitorSettings> options,
        ILogger<HardwareMonitor> logger)
    {
        Options = options.Value;
        Logger = logger;
        Job = new ProgramJob<string>(
            period: 5.Minutes(),
            method: GetHardwareAnalyzeResultsAsync,
            startUtcDate: DateTime.UtcNow + 5.Seconds(),
            description: Constants.HardwareWarningsInformer);
    }

    protected abstract Task<float> GetCpuTemperature();
    protected abstract Task<float> Get15MinAvgCpuUsage();
    protected abstract Task<double> GetMemoryUsagePercent();

    private async Task<string> GetHardwareAnalyzeResultsAsync()
    {
        var analyzeCpuTemperature = AnalyzeCpuTemperature();
        var analyzeCpuUsage = AnalyzeCpuUsage();
        var analyzeMemoryUsage = AnalyzeMemoryUsage();

        await Task.WhenAll(analyzeCpuTemperature, analyzeCpuUsage, analyzeMemoryUsage);

        var analyzeResult = analyzeCpuTemperature.Result + NewLine
                            + analyzeCpuUsage.Result + NewLine
                            + analyzeMemoryUsage.Result;

        return analyzeResult.Trim();
    }

    private async Task<string> AnalyzeCpuTemperature()
    {
        try
        {
            var cpuTemperature = await GetCpuTemperature();
            Logger.LogDebug("CPU temperature: {CPUTemperature}°C", cpuTemperature.ToString("0.##"));

            return cpuTemperature >= Options.WarnCpuTemperature
                ? $"CPU temperature: {cpuTemperature}°C"
                : string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogErrorIfNeed(ex, "Unable to get temperature sensors info: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
            return string.Empty;
        }
    }

    private async Task<string> AnalyzeMemoryUsage()
    {
        try
        {
            var memoryUsagePercent = await GetMemoryUsagePercent();
            Logger.LogDebug("Memory usage: {MemoryUsage}%", Math.Round(memoryUsagePercent, 0));

            return memoryUsagePercent > Options.WarnMemoryUsage
                ? $"Memory usage: {memoryUsagePercent:F0}%"
                : string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogErrorIfNeed(ex, "Unable to get memory usage: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
            return string.Empty;
        }
    }

    private async Task<string> AnalyzeCpuUsage()
    {
        try
        {
            var cpuUsage = await Get15MinAvgCpuUsage();

            Logger.LogDebug("CPU usage: {CpuUsage}", cpuUsage);

            return cpuUsage > Options.WarnCpuUsage
                ? $"15 min avg CPU usage: {cpuUsage}"
                : string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogErrorIfNeed(ex, "Unable to get CPU usage: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
            return string.Empty;
        }
    }

    public async Task<string> GetCurrentStateAsync()
    {
        var analyzeCpuTemperature = GetCpuTemperature();
        var analyzeCpuUsage = Get15MinAvgCpuUsage();
        var analyzeMemoryUsage = GetMemoryUsagePercent();

        await Task.WhenAll(analyzeCpuTemperature, analyzeCpuUsage, analyzeMemoryUsage);

        return $"CPU temperature: {analyzeCpuTemperature.Result}°C{NewLine}" +
               $"15 min avg CPU usage: {analyzeCpuUsage.Result}{NewLine}" +
               $"Memory usage: {analyzeMemoryUsage.Result:F0}%";
    }
}