using System;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Shell;
using static HomeBot.Features.HardwareMonitor.Constants;

namespace HomeBot.Features.HardwareMonitor;

public sealed class LinuxHardwareMonitor : HardwareMonitor
{
    public LinuxHardwareMonitor(
        IOptions<HardwareMonitorOptions> options,
        ILogger<LinuxHardwareMonitor> logger)
        : base(options, logger)
    {
    }

    protected override async Task<float> GetCpuTemperature()
    {
        // sudo apt install lm-sensors
        var commandResult = await ShellLauncher.RunAsync(Options.ShellPath, "sensors -j");

        EnsureResultSuccessful(commandResult);

        var jsonNode = JsonNode.Parse(commandResult.Value)!;
        return jsonNode["coretemp-isa-0000"]!["Package id 0"]!["temp1_input"]!.GetValue<float>();
    }

    protected override async Task<double> GetMemoryUsagePercent()
    {
        var commandResult = await ShellLauncher.RunAsync(Options.ShellPath, "egrep 'Mem|Cache|Swap' /proc/meminfo");
        // Approximate result:
        // MemTotal:       16067104 kB
        // MemAvailable:   12935852 kB
        // ...

        EnsureResultSuccessful(commandResult);

        var memUsage = commandResult.Value
            .Split("kB", StringSplitOptions.RemoveEmptyEntries)
            .Where(static row => !string.IsNullOrWhiteSpace(row.Trim()))
            .Select(static row => {
                var cells = row.Split(':');
                return new
                {
                    Name = cells[0].Trim(),
                    Size = int.Parse(cells[1].Trim())
                };
            })
            .ToArray();

        var total = memUsage.Single(static i => i.Name == MemTotal).Size;
        var available = memUsage.Single(static i => i.Name == MemAvailable).Size;

        return 100 - available / (double)total * 100;
    }

    protected override async Task<float> Get15MinAvgCpuUsage()
    {
        var commandResult = await ShellLauncher.RunAsync(Options.ShellPath, "cat /proc/loadavg | awk '{print $1\"-\"$2\"-\"$3}'");
        // Approximate result: 0.07-0.06-0.01

        EnsureResultSuccessful(commandResult);

        return commandResult.Value
            .Split('-')
            .Select(static i => float.Parse(i, CultureInfo.InvariantCulture))
            .ToArray()[2];
    }

    private void EnsureResultSuccessful(Result<string> result)
    {
        if (!result.Successful)
        {
            throw new FaultException(result.Fault!);
        }

        if (string.IsNullOrWhiteSpace(result.Value))
        {
            throw new FaultException(Fault.Unknown.WithMessage("Empty result"));
        }
    }
}