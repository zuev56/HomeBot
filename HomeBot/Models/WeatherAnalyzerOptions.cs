using System;
using System.ComponentModel.DataAnnotations;

namespace HomeBot.Models;

public sealed class WeatherAnalyzerOptions
{
    public const string SectionName = "WeatherAnalyzer";
    [Required]
    public DeviceOptions[] Devices { get; init; } = Array.Empty<DeviceOptions>();
}

public sealed class DeviceOptions
{
    [Required, Url]
    public string Uri { get; init; } = null!;
    [Required]
    public SensorOptions[] Sensors { get; init; } = Array.Empty<SensorOptions>();
    public string? Name { get; init; }
}

public sealed class SensorOptions
{
    [Required]
    public string Name { get; init; } = null!;
    [Required]
    public ParameterOptions[] Parameters { get; init; } = Array.Empty<ParameterOptions>();
}

public sealed class ParameterOptions
{
    [Required]
    public string Name { get; init; } = null!;
    [Required]
    public float HighLimit { get; init; }
    [Required]
    public float LowLimit { get; init; }
}