using System;
using System.Threading.Tasks;
using HomeBot.Features.Hardware;
using HomeBot.Features.Ping;
using HomeBot.Features.VkUsers;
using HomeBot.Features.Weather;

namespace HomeBot.Interaction;

internal sealed class SystemStatusService
{
    private readonly HardwareMonitor _hardwareMonitor;
    private readonly UserWatcher _userWatcher;
    private readonly WeatherAnalyzer _weatherAnalyzer;
    private readonly PingChecker _pingChecker;

    public SystemStatusService(HardwareMonitor hardwareMonitor, UserWatcher userWatcher, WeatherAnalyzer weatherAnalyzer, PingChecker pingChecker)
    {
        _hardwareMonitor = hardwareMonitor;
        _userWatcher = userWatcher;
        _weatherAnalyzer = weatherAnalyzer;
        _pingChecker = pingChecker;
    }

    public async Task<string> GetFullStatus()
    {
        var hardwareStatus = await GetHardwareStatusAsync();
        var usersStatus = await GetUsersStatusAsync();
        var weatherStatus = await GetWeatherStatusAsync();
        var pingStatus = await GetPingStatusAsync();
        var nl = Environment.NewLine;
        return $"Hardware:{nl}{hardwareStatus}{nl}{nl}" +
               $"Users:{nl}{usersStatus}{nl}{nl}" +
               $"Weather:{nl}{weatherStatus}{nl}{nl}" +
               $"Ping:{nl}{pingStatus}";
    }

    public async Task<string> GetHardwareStatusAsync() => await _hardwareMonitor.GetCurrentStateAsync();

    public async Task<string> GetUsersStatusAsync() => await _userWatcher.GetCurrentStateAsync();

    public async Task<string> GetWeatherStatusAsync() => await _weatherAnalyzer.GetCurrentStateAsync();

    public async Task<string> GetPingStatusAsync() => await _pingChecker.GetCurrentStateAsync();
}