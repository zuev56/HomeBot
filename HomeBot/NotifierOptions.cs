using System.ComponentModel.DataAnnotations;

namespace HomeBot;

internal sealed class NotifierOptions
{
    public const string SectionName = "Notifier";

    [Required]
    public int FromHour { get; set; }

    [Required]
    public int ToHour { get; set; }
}