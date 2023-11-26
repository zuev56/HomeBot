using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Messaging;
using Zs.Bot.Services.Pipeline;
using Zs.Common.Models;

namespace HomeBot.MessagePipeline;

internal sealed class Authorization : PipelineStep
{
    private readonly IBotClient _botClient;
    private readonly IReadOnlyList<long> _privilegedUserIds;

    public Authorization(IBotClient botClient, IReadOnlyList<long> privilegedUserIds)
    {
        _botClient = botClient;
        _privilegedUserIds = privilegedUserIds;
    }

    protected override async Task<Result> PerformInternalAsync(MessageActionData messageActionData, CancellationToken cancellationToken)
    {
        var (message, chat, user, action) = messageActionData;
        if (action != MessageAction.Received)
            return Result.Success();

        var userId = user?.Id;
        if (userId.HasValue && _privilegedUserIds.Contains(userId.Value))
            return Result.Success();

        var fault = Faults.Unauthorized;
        await _botClient.SendMessageAsync(fault.Code, chat!, messageToReply: message, cancellationToken);
        return fault;
    }
}