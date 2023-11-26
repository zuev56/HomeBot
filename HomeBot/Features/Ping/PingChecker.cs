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
    private readonly IOptions<PingCheckerSettings> _options;
    private readonly Dictionary<Device, bool> _hostToReachabilityMap = new();

    public ProgramJob<string> Job { get; }

    public PingChecker(IOptions<PingCheckerSettings> options)
    {
        _options = options;

        Job = new ProgramJob<string>(
            period: 20.Seconds(),
            method: PingAsync,
            startUtcDate: DateTime.UtcNow + 7.Seconds());
    }

    private async Task<string> PingAsync()
    {
        var message = new StringBuilder();
        foreach (var device in _options.Value.Devices)
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
        try
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            var pingReply = await ping.SendPingAsync(host).ConfigureAwait(false);

            return pingReply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
}