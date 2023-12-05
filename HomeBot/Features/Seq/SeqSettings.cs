using System;
using System.ComponentModel.DataAnnotations;

namespace HomeBot.Features.Seq;

public sealed class SeqSettings2 // TODO: remove Zs.Common.Services.Logging.Seq.SeqSettings
{
    public const string SectionName = "Seq";

    [Required]
    public string Url { get; init; } = null!;

    [Required]
    public string Token { get; init; } = null!;

    [Required]
    public int[] ObservedSignals { get; init; } = Array.Empty<int>();

    [Required]
    public int RequestedEventsCount { get; init; }
}