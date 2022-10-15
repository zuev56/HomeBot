using Zs.Common.Services.Abstractions;

namespace Home.Bot.Abstractions
{
    internal interface IHardwareMonitor
    {
        public IReadOnlyCollection<IJobBase> Jobs { get; }

        Task<float> Get15minAvgCpuUsage();
        Task<float> Get5minAvgCpuUsage();
        Task<float> GetCpuTemperature();
        Task<float> GetCpuUsage();
        Task<double> GetMemoryUsagePercent();
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);

    }
}
