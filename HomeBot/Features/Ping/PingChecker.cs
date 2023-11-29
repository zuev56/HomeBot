using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HomeBot.Features.Hardware;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Common.Services.Scheduling;

namespace HomeBot.Features.Ping;

internal sealed class PingChecker : IHasJob, IHasCurrentState
{
    private const int AttemptsWhenNotReachable = 3;
    private static readonly TimeSpan BaseDelay = 500.Milliseconds();
    private readonly PingCheckerSettings _settings;
    private readonly Dictionary<Target, bool> _targetToReachabilityMap = new();

    public ProgramJob<string> Job { get; }

    public PingChecker(IOptions<PingCheckerSettings> options)
    {
        _settings = options.Value;

        Job = new ProgramJob<string>(
            period: 20.Seconds(),
            method: PingTargetsAsync,
            startUtcDate: DateTime.UtcNow + 7.Seconds());
    }

    private async Task<string> PingTargetsAsync()
    {
        var message = new StringBuilder();
        foreach (var target in _settings.Targets)
        {
            var isReachable = await IsReachable(target);

            if (_targetToReachabilityMap.TryGetValue(target, out var lastIsReachable))
            {
                if (lastIsReachable == isReachable)
                    continue;

                _targetToReachabilityMap[target] = isReachable;

                if (message.Length > 0)
                    message.Append(Environment.NewLine);

                message.Append($"Connection to '{target.Description}' {(isReachable ? "restored" : "lost")}.");
            }
            else
                _targetToReachabilityMap.Add(target, isReachable);
        }

        return message.ToString();
    }

    private static async Task<bool> IsReachable(Target target)
    {
        var attempt = 0;
        while (attempt++ < AttemptsWhenNotReachable)
        {
            var pingStatus = await PingAsync(target);
            if (pingStatus == IPStatus.Success)
                return true;

            await Task.Delay(BaseDelay * attempt);
        }

        return false;
    }

    private static async Task<IPStatus> PingAsync(Target target)
    {
        return target.Port.HasValue
            ? Ping(target.Host, target.Port.Value)
            : await PingAsync(target.Host);
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

    private static IPStatus Ping(string host, int port)
    {
        try
        {
            using var client = new TcpClient(host, port);
            return IPStatus.Success;
        }
        catch
        {
            return IPStatus.Unknown;
        }
    }

    public async Task<string> GetCurrentStateAsync()
    {
        if (_settings.Targets.Length == 0)
            return string.Empty;

        var stateMessageBuilder = new StringBuilder();
        foreach (var target in _settings.Targets)
        {
            var hostStatus = await PingAsync(target);
            var targetName = target.Description ?? target.Host + (target.Port.HasValue ? $":{target.Port}" : string.Empty);
            stateMessageBuilder.AppendLine($"{targetName}: {hostStatus}");
        }

        return stateMessageBuilder.ToString().Trim();
    }
}