using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HomeBot.Features.Hardware;
using HomeBot.Features.Ping;
using HomeBot.Features.Seq;
using HomeBot.Features.VkUsers;
using HomeBot.Features.Weather;

namespace HomeBot.Interaction;

internal sealed class SystemStatusService
{
    private readonly HardwareMonitor _hardwareMonitor;
    private readonly UserWatcher _userWatcher;
    private readonly WeatherAnalyzer _weatherAnalyzer;
    private readonly PingChecker _pingChecker;
    private readonly SeqEventsInformer _seqEventsInformer;

    public SystemStatusService(
        HardwareMonitor hardwareMonitor,
        UserWatcher userWatcher,
        WeatherAnalyzer weatherAnalyzer,
        PingChecker pingChecker,
        SeqEventsInformer seqEventsInformer)
    {
        _hardwareMonitor = hardwareMonitor;
        _userWatcher = userWatcher;
        _weatherAnalyzer = weatherAnalyzer;
        _pingChecker = pingChecker;
        _seqEventsInformer = seqEventsInformer;
    }

    public async Task<string> GetFullStatus()
    {
        var sw = Stopwatch.StartNew();
        var getHardwareStatus = GetHardwareStatusAsync();
        var getUsersStatus = GetUsersStatusAsync();
        var getWeatherStatus = GetWeatherStatusAsync();
        var getPingStatus = GetPingStatusAsync();
        var getSeqStatus = GetSeqStatusAsync();

        await Task.WhenAll(getHardwareStatus, getUsersStatus, getWeatherStatus, getPingStatus, getSeqStatus);

        var nl = Environment.NewLine;
        return $"Hardware:{nl}{getHardwareStatus.Result}{nl}{nl}" +
               $"Users:{nl}{getUsersStatus.Result}{nl}{nl}" +
               $"Weather:{nl}{getWeatherStatus.Result}{nl}{nl}" +
               $"Ping:{nl}{getPingStatus.Result}{nl}{nl}" +
               $"Seq:{nl}{getSeqStatus.Result}{nl}{nl}" +
               $"Request time: {sw.ElapsedMilliseconds} ms";
    }

    public Task<string> GetHardwareStatusAsync() => _hardwareMonitor.GetCurrentStateAsync();

    public Task<string> GetUsersStatusAsync() => _userWatcher.GetCurrentStateAsync();

    public Task<string> GetWeatherStatusAsync() => _weatherAnalyzer.GetCurrentStateAsync();

    public Task<string> GetPingStatusAsync() => _pingChecker.GetCurrentStateAsync();

    public Task<string> GetSeqStatusAsync() => _seqEventsInformer.GetCurrentStateAsync();
}