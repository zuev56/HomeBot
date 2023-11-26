using System;
using Zs.Bot.Data.Models;
using Zs.Bot.Telegram.Extensions;

namespace HomeBot;

internal static class MessageHelper
{
    public static readonly Func<Message, string?> GetMessageText = static message => message.GetText();
}