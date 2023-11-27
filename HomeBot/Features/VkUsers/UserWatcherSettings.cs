using System;
using System.ComponentModel.DataAnnotations;

namespace HomeBot.Features.VkUsers;

internal sealed class UserWatcherSettings
{
    public const string SectionName = "UserWatcher";

    [Required]
    public string VkActivityApiUri { get; set; } = null!;

    [Required]
    public int[] TrackedIds { get; set; } = Array.Empty<int>();

    [Required]
    public double InactiveHoursLimit { get; set; }
}