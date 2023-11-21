using System.ComponentModel.DataAnnotations;

namespace HomeBot.Features.Ping;

public sealed record Device
{
    [Required]
    public required string Host { get; init; } = null!;

    public string? Description { get; init; }
}