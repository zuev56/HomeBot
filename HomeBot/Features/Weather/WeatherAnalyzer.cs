using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeBot.Features.Hardware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Common.Services.Scheduling;
using Zs.EspMeteo.Parser;
using Zs.EspMeteo.Parser.Models;

namespace HomeBot.Features.Weather;

internal sealed class WeatherAnalyzer : IHasJob, IHasCurrentState
{
    private readonly EspMeteoParser _espMeteoParser;
    private readonly WeatherAnalyzerSettings _settings;
    private readonly ILogger<WeatherAnalyzer>? _logger;
    private readonly TimeSpan _alarmInterval = 2.Hours();
    private DateTime? _lastAlarmDate = DateTime.UtcNow - 2.Hours();

    public ProgramJob<string> Job { get; }

    public WeatherAnalyzer(
        EspMeteoParser espMeteoParser,
        IOptions<WeatherAnalyzerSettings> options,
        ILogger<WeatherAnalyzer>? logger)
    {
        _espMeteoParser = espMeteoParser;
        _settings = options.Value;
        _logger = logger;

        Job = new ProgramJob<string>(
            period: 5.Minutes(),
            method: AnalyzeAsync,
            startUtcDate: DateTime.UtcNow + 10.Seconds()
        );
    }

    private async Task<string> AnalyzeAsync()
    {
        // Временный костыль
        if (DateTime.UtcNow < _lastAlarmDate + _alarmInterval)
            return string.Empty;

        var espMeteos = await GetEspMeteoInfosAsync();
        var deviations = GetDeviationInfos(espMeteos).Trim();

        // Временный костыль
        if (!string.IsNullOrEmpty(deviations))
            _lastAlarmDate = DateTime.UtcNow;

        return deviations;
    }

    private async Task<EspMeteo[]> GetEspMeteoInfosAsync()
    {
        var parseTasks = _settings.Devices
            .Select(static d => d.Uri)
            .Select(url => _espMeteoParser.ParseAsync(url));

        var espMeteos = await Task.WhenAll(parseTasks);
        return espMeteos;
    }

    private string GetDeviationInfos(IEnumerable<EspMeteo> espMeteos)
    {
        var allDeviations = new StringBuilder();
        foreach (var espMeteo in espMeteos)
        {
            var settings = _settings.Devices.Single(s => s.Uri == espMeteo.Uri);
            var deviations = AnalyzeDeviations(espMeteo, settings);
            if (string.IsNullOrEmpty(deviations))
                continue;

            allDeviations.AppendLine();
            allDeviations.AppendLine(deviations);
        }

        return allDeviations.ToString();
    }

    private static string AnalyzeDeviations(EspMeteo espMeteo, DeviceSettings deviceSettings)
    {
        var espMeteoDeviceName = $"[{deviceSettings.Name ?? espMeteo.Uri}].";
        var deviations = new StringBuilder(espMeteoDeviceName);

        foreach (var sensor in espMeteo.Sensors)
        {
            var sensorSettings = deviceSettings.Sensors.SingleOrDefault(s => s.Name == sensor.Name);
            if (sensorSettings is null)
                continue;

            foreach (var parameter in sensor.Parameters)
            {
                var parameterSettings = sensorSettings.Parameters.SingleOrDefault(s => s.Name == parameter.Name);
                if (parameterSettings is null)
                    continue;

                if (parameter.Value > parameterSettings.HighLimit)
                {
                    var deviation = $"{sensorSettings.Alias ?? sensor.Name}.{parameter.Name}: value {parameter.Value} {parameter.Unit} is higher than limit {parameterSettings.HighLimit} {parameter.Unit}";
                    deviations.AppendLine(deviation);
                }

                if (parameter.Value < parameterSettings.LowLimit)
                {
                    var deviation = $"{sensorSettings.Alias ?? sensor.Name}.{parameter.Name}: value {parameter.Value} {parameter.Unit} is lower than limit {parameterSettings.LowLimit} {parameter.Unit}";
                    deviations.AppendLine(deviation);
                }
            }
        }

        var result = deviations.ToString();
        return result == espMeteoDeviceName ? string.Empty : result;
    }

    public async Task<string> GetCurrentStateAsync()
    {
        var espMeteos = await GetEspMeteoInfosAsync();
        var states = espMeteos.SelectMany(espMeteo =>
        {
            var deviceSettings = _settings.Devices.Single(s => s.Uri == espMeteo.Uri);
            return espMeteo.Sensors.SelectMany(sensor =>
            {
                var sensorSettings = deviceSettings.Sensors.SingleOrDefault(s => s.Name == sensor.Name);
                return sensor.Parameters.Select(parameter => $"{sensorSettings?.Alias ?? sensor.Name}.{parameter.Name}: {parameter.Value}");
            });
        });

        return string.Join(Environment.NewLine, states);
    }
}