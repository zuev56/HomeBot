using System;
using System.ComponentModel.DataAnnotations;

namespace HomeBot.Features.Weather;

internal sealed class WeatherAnalyzerSettings
{
    public const string SectionName = "WeatherAnalyzer";
    [Required]
    public DeviceSettings[] Devices { get; init; } = Array.Empty<DeviceSettings>();
}

internal sealed class DeviceSettings
{
    [Required, Url]
    public string Uri { get; init; } = null!;
    [Required]
    public SensorSettings[] Sensors { get; init; } = Array.Empty<SensorSettings>();
    public string? Name { get; init; }
}

internal sealed class SensorSettings
{
    [Required]
    public string Name { get; init; } = null!;
    public string? Alias { get; init; }
    [Required]
    public ParameterSettings[] Parameters { get; init; } = Array.Empty<ParameterSettings>();
}

internal sealed class ParameterSettings
{
    [Required]
    public string Name { get; init; } = null!;
    [Required]
    public float HighLimit { get; init; }
    [Required]
    public float LowLimit { get; init; }
}