using System.ComponentModel.DataAnnotations;

namespace HomeBot.Interaction;

internal sealed class NotifierSettings
{
    public const string SectionName = "Notifier";

    [Required]
    public int FromHour { get; init; }

    [Required]
    public int ToHour { get; init; }
}