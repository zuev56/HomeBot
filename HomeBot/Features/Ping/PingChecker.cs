using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Common.Services.Scheduling;

namespace HomeBot.Features.Ping;

internal sealed class PingChecker
{
    private const int AttemptsWhenNotReachable = 3;
    private static readonly TimeSpan BaseDelay = 500.Milliseconds();
    private readonly PingCheckerSettings _settings;
    private readonly Dictionary<Device, bool> _hostToReachabilityMap = new();

    public ProgramJob<string> Job { get; }

    public PingChecker(IOptions<PingCheckerSettings> options)
    {
        _settings = options.Value;

        Job = new ProgramJob<string>(
            period: 20.Seconds(),
            method: PingDevicesAsync,
            startUtcDate: DateTime.UtcNow + 7.Seconds());
    }

    private async Task<string> PingDevicesAsync()
    {
        var message = new StringBuilder();
        foreach (var device in _settings.Devices)
        {
            var isReachable = await IsReachable(device.Host);

            if (_hostToReachabilityMap.TryGetValue(device, out var lastIsReachable))
            {
                if (lastIsReachable == isReachable)
                    continue;

                _hostToReachabilityMap[device] = isReachable;

                if (message.Length > 0)
                    message.Append(Environment.NewLine);

                message.Append($"Connection to '{device.Description}' {(isReachable ? "restored" : "lost")}.");
            }
            else
                _hostToReachabilityMap.Add(device, isReachable);
        }

        return message.ToString();
    }

    private static async Task<bool> IsReachable(string host)
    {
        var attempt = 0;
        while (attempt++ < AttemptsWhenNotReachable)
        {
            var pingStatus = await PingAsync(host);
            if (pingStatus == IPStatus.Success)
                return true;

            await Task.Delay(BaseDelay * attempt);
        }

        return false;
    }

    private static async Task<IPStatus> PingAsync(string host)
    {
        try
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            var pingReply = await ping.SendPingAsync(host).ConfigureAwait(false);

            return pingReply.Status;
        }
        catch
        {
            return IPStatus.Unknown;
        }
    }

    public async Task<string> GetCurrentStateAsync()
    {
        if (_settings.Devices.Length == 0)
            return string.Empty;

        var stateMessageBuilder = new StringBuilder();
        foreach (var device in _settings.Devices)
        {
            var hostStatus = await PingAsync(device.Host);
            stateMessageBuilder.AppendLine($"{device.Description ?? device.Host}: {hostStatus}");
        }

        return stateMessageBuilder.ToString();
    }
}