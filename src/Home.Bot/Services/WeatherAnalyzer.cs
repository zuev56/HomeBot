using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Home.Bot.Models;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.EspMeteo.Parser;
using Zs.EspMeteo.Parser.Models;

namespace Home.Bot.Services;

internal sealed class WeatherAnalyzer
{
    private readonly EspMeteoParser _espMeteoParser;
    private readonly WeatherAnalyzerOptions _weatherAnalyzerOptions;
    private readonly ILogger<WeatherAnalyzer>? _logger;

    private readonly TimeSpan _alarmInterval = 2.Hours();
    private DateTime? _lastAlarmDate = DateTime.UtcNow - 2.Hours();

    public WeatherAnalyzer(
        EspMeteoParser espMeteoParser,
        WeatherAnalyzerOptions weatherAnalyzerOptions,
        ILogger<WeatherAnalyzer>? logger)
    {
        _espMeteoParser = espMeteoParser;
        _weatherAnalyzerOptions = weatherAnalyzerOptions;
        _logger = logger;
    }

    public async Task<string> AnalyzeAsync()
    {
        // Временный костыль
        if (DateTime.UtcNow < _lastAlarmDate + _alarmInterval)
        {
            return string.Empty;
        }

        var espMeteoInfos = await GetEspMeteoInfosAsync();
        var deviations = GetDeviationsInfo(espMeteoInfos).Trim();

        // Временный костыль
        if (!string.IsNullOrEmpty(deviations))
        {
            _lastAlarmDate = DateTime.UtcNow;
        }

        return deviations;
    }

    private async Task<EspMeteo[]> GetEspMeteoInfosAsync()
    {
        var parseTasks = _weatherAnalyzerOptions.Devices
            .Select(static d => d.Uri)
            .Select(url => _espMeteoParser.ParseAsync(url));

        var espMeteos = await Task.WhenAll(parseTasks);
        return espMeteos;
    }

    private string GetDeviationsInfo(IEnumerable<EspMeteo> espMeteoInfos)
    {
        var deviations = new StringBuilder();
        foreach (var espMeteoInfo in espMeteoInfos)
        {
            var settings = _weatherAnalyzerOptions.Devices.Single(s => s.Uri == espMeteoInfo.Uri);
            var deviceDeviations = AnalyzeDeviations(espMeteoInfo, settings);
            if (string.IsNullOrEmpty(deviceDeviations))
            {
                continue;
            }

            deviations.AppendLine();
            deviations.AppendLine(deviceDeviations);
        }

        return deviations.ToString();
    }

    private static string AnalyzeDeviations(EspMeteo espMeteoInfo, DeviceOptions deviceOptions)
    {
        var espMeteoDeviceName = $"[{deviceOptions.Name ?? espMeteoInfo.Uri}].";
        var deviations = new StringBuilder(espMeteoDeviceName);

        foreach (var sensor in espMeteoInfo.Sensors)
        {
            var sensorSettings = deviceOptions.Sensors.SingleOrDefault(s => s.Name == sensor.Name);
            if (sensorSettings is null)
            {
                continue;
            }

            foreach (var parameter in sensor.Parameters)
            {
                var parameterSettings = sensorSettings.Parameters.SingleOrDefault(s => s.Name == parameter.Name);
                if (parameterSettings is null)
                {
                    continue;
                }

                if (parameter.Value > parameterSettings.HighLimit)
                {
                    var deviation = $"[{sensor.Name}].[{parameter.Name}]: value {parameter.Value} {parameter.Unit} is higher than limit {parameterSettings.HighLimit} {parameter.Unit}";
                    deviations.AppendLine(deviation);
                }

                if (parameter.Value < parameterSettings.LowLimit)
                {
                    var deviation = $"[{sensor.Name}].[{parameter.Name}]: value {parameter.Value} {parameter.Unit} is lower than limit {parameterSettings.LowLimit} {parameter.Unit}";
                    deviations.AppendLine(deviation);
                }
            }
        }

        var result = deviations.ToString();
        return result == espMeteoDeviceName ? string.Empty : result;
    }
}