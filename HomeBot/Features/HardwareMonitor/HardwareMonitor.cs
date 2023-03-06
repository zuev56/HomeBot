using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Scheduling;

namespace HomeBot.Features.HardwareMonitor;

public abstract class HardwareMonitor
{
    protected readonly HardwareMonitorOptions Options;
    protected readonly ILogger<HardwareMonitor> Logger;

    public ProgramJob<string> Job { get; }

    protected HardwareMonitor(
        IOptions<HardwareMonitorOptions> options,
        ILogger<HardwareMonitor> logger)
    {
        Options = options.Value;
        Logger = logger;
        Job = new ProgramJob<string>(
            period: TimeSpan.FromMinutes(5),
            method: GetHardwareAnalyzeResults,
            startUtcDate: DateTime.UtcNow + TimeSpan.FromSeconds(5),
            description: Constants.HardwareWarningsInformer);
    }

    public void Start()
    {
        Logger.LogInformationIfNeed($"{nameof(HardwareMonitor)} started");
    }

    public void Stop()
    {
        Logger.LogInformationIfNeed($"{nameof(HardwareMonitor)} stopped");
    }

    protected abstract Task<float> GetCpuTemperature();
    protected abstract Task<float> Get15MinAvgCpuUsage();
    protected abstract Task<double> GetMemoryUsagePercent();

    private async Task<string> GetHardwareAnalyzeResults()
    {
        var analyzeCpuTemperature = AnalyzeCpuTemperature();
        var analyzeCpuUsage = AnalyzeCpuUsage();
        var analyzeMemoryUsage = AnalyzeMemoryUsage();

        await Task.WhenAll(analyzeCpuTemperature, analyzeCpuUsage, analyzeMemoryUsage);

        var cpuTemperatureInfo = analyzeCpuTemperature.Result.Successful
            ? analyzeCpuTemperature.Result.Value
            : analyzeCpuTemperature.Result.Fault!.Message;
        var cpuUsageInfo = analyzeCpuUsage.Result.Successful
            ? analyzeCpuUsage.Result.Value
            : analyzeCpuUsage.Result.Fault!.Message;
        var memoryUsageInfo = analyzeMemoryUsage.Result.Successful
            ? analyzeMemoryUsage.Result.Value
            : analyzeMemoryUsage.Result.Fault!.Message;

        var newLine = Environment.NewLine;
        var analyzeResult = cpuTemperatureInfo + newLine + newLine
                            + cpuUsageInfo + newLine + newLine
                            + memoryUsageInfo;

        return analyzeResult;
    }

    private async Task<Result<string>> AnalyzeCpuTemperature()
    {
        try
        {
            var cpuTemperature = await GetCpuTemperature();

            Logger.LogDebug("CPU temperature: {CPUTemperature}°C", cpuTemperature.ToString("0.##"));

            return cpuTemperature >= Options.WarnCpuTemperature
                ? Result.Success($"CPU temperature: {cpuTemperature}°C")
                : Result.Success(string.Empty);
        }
        catch (Exception ex)
        {
            Logger.LogErrorIfNeed(ex, "Unable to get temperature sensors info: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
            return Result.Fail<string>(Fault.Unknown.WithMessage($"Unable to get temperature sensors info: {ex.Message}"));
        }
    }

    private async Task<Result<string>> AnalyzeMemoryUsage()
    {
        try
        {
            var memoryUsagePercent = await GetMemoryUsagePercent();

            Logger.LogDebug("Memory usage: {MemoryUsage}%", Math.Round(memoryUsagePercent, 0));

            return memoryUsagePercent > Options.WarnMemoryUsage
                ? Result.Success($"Memory usage: {memoryUsagePercent:F0}%")
                : Result.Success(string.Empty);
        }
        catch (Exception ex)
        {
            Logger.LogErrorIfNeed(ex, "Unable to get memory usage: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
            return Result.Fail<string>(Fault.Unknown.WithMessage($"Unable to get memory usage: {ex.Message}"));
        }
    }

    private async Task<Result<string>> AnalyzeCpuUsage()
    {
        try
        {
            var cpuUsage = await Get15MinAvgCpuUsage();

            Logger.LogDebug("CPU usage: {CpuUsage}", cpuUsage);

            return cpuUsage > Options.WarnCpuUsage
                ? Result.Success($"15 min avg CPU usage: {cpuUsage}")
                : Result.Success(string.Empty);
        }
        catch (Exception ex)
        {
            Logger.LogErrorIfNeed(ex, "Unable to get CPU usage: {ExceptionType}\n{Message}\n{StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);
            return Result.Fail<string>(Fault.Unknown.WithMessage($"Unable to get CPU usage: {ex.Message}"));
        }
    }
}