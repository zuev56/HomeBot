using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Services.Scheduling;

namespace HomeBot.Abstractions;

internal interface IHardwareMonitor
{
    public IReadOnlyCollection<JobBase> Jobs { get; }

    Task<float> Get15minAvgCpuUsage();
    Task<float> Get5minAvgCpuUsage();
    Task<float> GetCpuTemperature();
    Task<float> GetCpuUsage();
    Task<double> GetMemoryUsagePercent();
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);

}