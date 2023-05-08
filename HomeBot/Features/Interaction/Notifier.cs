using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Enums;
using Zs.Bot.Services.Messaging;
using Zs.Common.Extensions;

namespace HomeBot.Features.Interaction;

internal sealed class Notifier
{
    private readonly IMessagesRepository _messagesRepository;
    private readonly IMessenger _messenger;
    private readonly NotifierOptions _notifierOptions;

    public Notifier(
        IMessagesRepository messagesRepository,
        IMessenger messenger,
        IOptions<NotifierOptions> notifierOptions)
    {
        _messagesRepository = messagesRepository;
        _messenger = messenger;
        _notifierOptions = notifierOptions.Value;
    }

    public async Task ForceNotifyAsync(string notification)
    {
        var preparedMessage = GetPreparedMessage(notification);
        await _messenger.AddMessageToOutboxAsync(preparedMessage, Role.Owner, Role.Admin);
    }

    private static string GetPreparedMessage(string message) => message.ReplaceEndingWithThreeDots(4000);

    public async Task NotifyAsync(string jobResult)
    {
        var curHour = DateTime.Now.Hour;
        if (string.IsNullOrWhiteSpace(jobResult) || curHour < _notifierOptions.FromHour || curHour >= _notifierOptions.ToHour)
        {
            return;
        }

        var preparedMessage = GetPreparedMessage(jobResult);
        await ForceNotifyAsync(preparedMessage);
    }

    public async Task NotifyOnceADayAsync(string message, string separateTemplate)
    {
        var preparedMessage = GetPreparedMessage(message);
        var todayAlerts = await _messagesRepository.FindAllTodayMessagesWithTextAsync(separateTemplate);
        if (todayAlerts.All(m => m.Text?.WithoutDigits() != message.WithoutDigits()))
        {
            await NotifyAsync(preparedMessage);
        }
    }
}