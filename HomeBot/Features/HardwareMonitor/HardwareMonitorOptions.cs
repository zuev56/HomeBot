using System.ComponentModel.DataAnnotations;

namespace HomeBot.Features.HardwareMonitor;

public sealed class HardwareMonitorOptions
{
    public static string SectionName = "HardwareMonitor";

    [Required]
    public string ShellPath { get; init; } = null!;

    [Required]
    public float WarnMemoryUsage { get; init; }

    [Required]
    public float WarnCpuUsage { get; init; }

    [Required]
    public float WarnCpuTemperature { get; init; }
}