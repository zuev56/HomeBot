using System;
using System.Linq;
using System.Threading.Tasks;
using static HomeBot.Interaction.MessagePipeline.Commands;

namespace HomeBot.Interaction;

internal sealed class CommandHandler
{
    private readonly Notifier _notifier;
    private readonly SystemStatusService _systemStatusService;

    public CommandHandler(Notifier notifier, SystemStatusService systemStatusService)
    {
        _notifier = notifier;
        _systemStatusService = systemStatusService;
    }

    public async Task<bool> TryHandleAsync(string? command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var knownCommands = new[] {FullStatus, WeatherStatus, HardwareStatus, UserStatus, PingStatus};
        if (!knownCommands.Contains(command))
            return false;

        var response = command switch
        {
            FullStatus => await _systemStatusService.GetFullStatus(),
            WeatherStatus => await _systemStatusService.GetWeatherStatusAsync(),
            HardwareStatus => await _systemStatusService.GetHardwareStatusAsync(),
            UserStatus => await _systemStatusService.GetUsersStatusAsync(),
            PingStatus => await _systemStatusService.GetPingStatusAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };

        await _notifier.ForceNotifyAsync(response);

        return true;
    }
}