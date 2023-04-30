using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Enums;
using Zs.Bot.Services.Messaging;
using Zs.Common.Extensions;
using static HomeBot.Features.UserWatcher.Constants;

namespace HomeBot.Features.Notification;

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

    public async Task NotifyAsync(string notification)
    {
        await _messenger.AddMessageToOutboxAsync(notification, Role.Owner, Role.Admin);
    }

    public async Task NotifyAsync(string? jobDescription, string jobResult)
    {
        var curHour = DateTime.Now.Hour;
        if (string.IsNullOrWhiteSpace(jobResult) || curHour < _notifierOptions.FromHour ||
            curHour >= _notifierOptions.ToHour)
        {
            return;
        }

        var preparedMessage = jobResult.ReplaceEndingWithThreeDots(4000);

        if (jobDescription == InactiveUsersInformer)
        {
            await NotifyAboutInactiveUsersAsync(preparedMessage);
        }
        else
        {
            await NotifyAsync(preparedMessage);
        }
    }

    private async Task NotifyAboutInactiveUsersAsync(string preparedMessage)
    {
        var todayAlerts = await _messagesRepository.FindAllTodayMessagesWithTextAsync("is not active for");
        if (todayAlerts.All(m => m.Text?.WithoutDigits() != preparedMessage.WithoutDigits()))
        {
            var text = string.Join(Environment.NewLine + Environment.NewLine, todayAlerts.Select(a => a.Text));
            Console.WriteLine(text);
            await NotifyAsync(preparedMessage);
        }
    }
}