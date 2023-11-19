using System;
using System.Linq;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.Messaging;
using static HomeBot.Features.Interaction.Commands;

namespace HomeBot.Features.Interaction;

internal sealed class MessageHandler
{
    private readonly IMessenger _messenger;
    private readonly Notifier _notifier;
    private readonly SystemStatusService _systemStatusService;

    public MessageHandler(IMessenger messenger, Notifier notifier, SystemStatusService systemStatusService)
    {
        _messenger = messenger;
        _notifier = notifier;
        _systemStatusService = systemStatusService;
        _messenger.MessageReceived += Messenger_MessageReceived;
    }

    private async void Messenger_MessageReceived(object? sender, MessageActionEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(nameof(e.Message));

        var message = e.Message!.Text?.Trim().ToLower() ?? string.Empty;
        if (!BotCommand.IsCommand(message))
            return;

        if (new[] { FullStatus, WeatherStatus, HardwareStatus, UserStatus }.Contains(message))
        {
            e.IsHandled = true;

            var response = message switch
            {
                FullStatus => await _systemStatusService.GetFullStatus(),
                WeatherStatus => await _systemStatusService.GetWeatherStatusAsync(),
                HardwareStatus => await _systemStatusService.GetHardwareStatusAsync(),
                UserStatus => await _systemStatusService.GetUsersStatusAsync(),
                _ => throw new ArgumentOutOfRangeException(nameof(message))
            };

            await _notifier.ForceNotifyAsync(response);
        }
    }
}