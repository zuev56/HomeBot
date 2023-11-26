using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeBot;

internal sealed class BotSettings
{
    public const string SectionName = "Bot";

    [Required]
    public string Token { get; init; } = null!;

    public string? Name { get; init; }

    [Required]
    public long OwnerChatRawId { get; init; }

    [Required]
    public string CliPath { get; init; } = null!;

    [Required]
    public IReadOnlyList<long> PrivilegedUserRawIds { get; init; } = Array.Empty<long>();
}