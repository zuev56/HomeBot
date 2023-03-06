using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Services.Scheduling;

namespace HomeBot.Features.HardwareMonitor;

internal interface IHardwareMonitor
{
    public IReadOnlyCollection<JobBase> Jobs { get; }

    Task<float> Get15MinAvgCpuUsage();
    Task<float> Get5MinAvgCpuUsage();
    Task<float> GetCpuTemperature();
    Task<float> GetCpuUsage();
    Task<double> GetMemoryUsagePercent();
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);

}