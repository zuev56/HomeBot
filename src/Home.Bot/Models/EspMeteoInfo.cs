namespace Home.Bot.Models;

public sealed record EspMeteoDeviceSettings(string EspMeteoUri, SensorSettings[] SensorInfos, string? DeviceName = null);

public sealed record SensorSettings(string SensorName, ParameterSettings[] ParameterInfos);

public sealed record ParameterSettings(string ParameterName, (float High, float Low) Limits);