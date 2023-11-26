using System;
using System.ComponentModel.DataAnnotations;

namespace HomeBot.Features.Ping;

public sealed class PingCheckerSettings
{
    public const string SectionName = "PingChecker";

    [Required]
    public Device[] Devices { get; init; } = Array.Empty<Device>();
}