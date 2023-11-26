using Zs.Common.Models;

namespace HomeBot;

internal static class Faults
{
    public static Fault Unauthorized => new(nameof(Unauthorized));
    public static Fault CommandHandled => new(nameof(CommandHandled));
}