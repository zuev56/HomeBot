using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.Messaging;
using Zs.Bot.Services.Pipeline;
using Zs.Common.Models;
using static HomeBot.Faults;

namespace HomeBot.Interaction.MessagePipeline;

internal sealed class MessageHandler : PipelineStep
{
    private readonly CommandHandler _commandHandler;

    public MessageHandler(CommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
    }

    protected override async Task<Result> PerformInternalAsync(MessageActionData messageActionData, CancellationToken cancellationToken)
    {
        var (message, _, _, action) = messageActionData;
        if (action != MessageAction.Received)
            return Result.Success();

        var messageText = BotSettings.GetMessageText(message!)?.Trim().ToLower();
        if (!messageText.IsBotCommand())
            return Result.Success();

        var handled = await _commandHandler.TryHandleAsync(messageText);

        return handled ? CommandHandled : Result.Success();
    }
}