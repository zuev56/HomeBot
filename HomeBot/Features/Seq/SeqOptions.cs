using System;
using System.ComponentModel.DataAnnotations;

namespace HomeBot.Features.Seq;

public sealed class SeqOptions
{
    public static string SectionName = "Seq";

    [Required]
    public string Url { get; init; } = null!;

    [Required]
    public string Token { get; init; } = null!;

    [Required]
    public int[] ObservedSignals { get; init; } = Array.Empty<int>();
}