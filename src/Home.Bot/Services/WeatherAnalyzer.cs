using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Home.Bot.Models;
using Microsoft.Extensions.Logging;
using Zs.EspMeteo.Parser;
using Zs.EspMeteo.Parser.Models;

namespace Home.Bot.Services;

internal sealed class WeatherAnalyzer
{
    private readonly EspMeteoParser _espMeteoParser;
    private readonly EspMeteoDeviceSettings[] _deviceSettings;
    private readonly ILogger<WeatherAnalyzer> _logger;

    public WeatherAnalyzer(
        EspMeteoParser espMeteoParser,
        EspMeteoDeviceSettings[] deviceSettings,
        ILogger<WeatherAnalyzer> logger)
    {
        _espMeteoParser = espMeteoParser;
        _deviceSettings = deviceSettings;
        _logger = logger;
    }

    public async Task<string> AnalyzeAsync()
    {
        var espMeteoInfos = await GetEspMeteoInfosAsync();
        var deviations = GetDeviationsInfo(espMeteoInfos);

        return deviations;
    }

    private async Task<EspMeteo[]> GetEspMeteoInfosAsync()
    {
        var parseTasks = _deviceSettings
            .Select(s => s.EspMeteoUri)
            .Select(url => _espMeteoParser.ParseAsync(url));

        var espMeteos = await Task.WhenAll(parseTasks);
        return espMeteos;
    }

    private string? GetDeviationsInfo(IEnumerable<EspMeteo> espMeteoInfos)
    {
        var deviations = new StringBuilder();
        foreach (var espMeteoInfo in espMeteoInfos)
        {
            var settings = _deviceSettings.Single(s => s.EspMeteoUri == espMeteoInfo.Uri);
            var deviceDeviations = AnalyzeDeviations(espMeteoInfo, settings);

            if (deviceDeviations is not null)
            {
                deviations.AppendLine();
                deviations.AppendLine(deviceDeviations);
            }
        }

        return deviations.Length == 0 ? null : deviations.ToString();
    }

    private static string? AnalyzeDeviations(EspMeteo espMeteoInfo, EspMeteoDeviceSettings espMeteoSettings)
    {
        var espMeteoDeviceName = $"{espMeteoSettings.DeviceName ?? espMeteoInfo.Uri}";
        var deviations = new StringBuilder(espMeteoDeviceName);

        foreach (var sensor in espMeteoInfo.Sensors)
        {
            var sensorSettings = espMeteoSettings.SensorInfos.SingleOrDefault(s => s.SensorName == sensor.Name);
            if (sensorSettings is null)
            {
                continue;
            }

            foreach (var parameter in sensor.Parameters)
            {
                var parameterSettings = sensorSettings.ParameterInfos.SingleOrDefault(s => s.ParameterName == parameter.Name);
                if (parameterSettings is null)
                {
                    continue;
                }

                if (parameter.Value > parameterSettings.Limits.High)
                {
                    var deviation = $"[{sensor.Name}].[{parameter.Name}]: value {parameter.Value} is higher than limit {parameterSettings.Limits.High}";
                    deviations.AppendLine(deviation);
                }

                if (parameter.Value < parameterSettings.Limits.Low)
                {
                    var deviation = $"[{sensor.Name}].[{parameter.Name}]: value {parameter.Value} is lower than limit {parameterSettings.Limits.Low}";
                    deviations.AppendLine(deviation);
                }
            }
        }

        var result = deviations.ToString();
        return result == espMeteoDeviceName ? null : result;
    }
}