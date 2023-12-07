using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeBot.Features.Hardware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Common.Services.Logging.Seq;
using Zs.Common.Services.Scheduling;
using static System.Environment;

namespace HomeBot.Features.Seq;

public sealed class SeqEventsInformer : IHasCurrentState
{
    private const int UtcToMsk = +3;
    private readonly SeqSettings2 _settings;
    private readonly ISeqService _seqService;
    public ProgramJob<string> DayEventsInformerJob { get; }
    public ProgramJob<string> NightEventsInformerJob { get; }

    public SeqEventsInformer(
        IOptions<SeqSettings2> options,
        ISeqService seqService,
        ILogger<SeqEventsInformer> logger)
    {
        _settings = options.Value;
        _seqService = seqService;

        DayEventsInformerJob = new ProgramJob<string>(
            period: 1.Hours(),
            method: () => GetSeqEventsAsync(DateTime.UtcNow - 1.Hours()),
            startUtcDate: DateTime.UtcNow.NextHour(),
            description: "dayErrorsAndWarningsInformer",
            logger: logger
        );

        NightEventsInformerJob = new ProgramJob<string>(
            period: 1.Days(),
            method: () => GetSeqEventsAsync(DateTime.UtcNow - 12.Hours()),
            startUtcDate: DateTime.UtcNow.Date + (24 + 10).Hours(),
            description: "nightErrorsAndWarningsInformer",
            logger: logger
        );
    }

    private async Task<string> GetSeqEventsAsync(DateTime fromDate)
    {
        var seqEvents = await _seqService.GetLastEventsAsync(_settings.RequestedEventsCount, _settings.ObservedSignals)
            .ContinueWith(task => task.Result.Where(seqEvent => seqEvent.Timestamp > fromDate).ToList());

        return seqEvents.Count > 0
            ? CreateMessageFromSeqEvents(seqEvents)
            : string.Empty;
    }

    private string CreateMessageFromSeqEvents(IEnumerable<SeqEvent> seqEvents)
    {
        var messageBuilder = new StringBuilder();

        foreach (var seqEvent in seqEvents)
        {
            var localDate = seqEvent.Timestamp.AddHours(UtcToMsk).ToString("dd.MM.yyyy HH:mm:ss");
            var applicationName = seqEvent.Parameters["ApplicationName"].ToString();

            messageBuilder.Append(localDate).Append(' ').Append(seqEvent.Level.ToUpperInvariant()).AppendLine(": ")
                .AppendLine(seqEvent.Message);

            if (!string.IsNullOrWhiteSpace(seqEvent.Exception))
                messageBuilder.Append("Exception: ").AppendLine(seqEvent.Exception[..seqEvent.Exception.IndexOf(NewLine, StringComparison.Ordinal)]);

            messageBuilder.Append($"App: {applicationName}")
                .AppendLine().AppendLine();
        }

        return messageBuilder.ToString().ReplaceEndingWithThreeDots(maxStringLength: 4000);
    }

    public async Task<string> GetCurrentStateAsync()
    {
        var seqEvents = await _seqService.GetLastEventsAsync(_settings.RequestedEventsCount, _settings.ObservedSignals);
        var lastWeek = seqEvents.Count(e => e.Timestamp > DateTime.UtcNow.AddDays(-7));
        var last24Hours = seqEvents.Count(e => e.Timestamp > DateTime.UtcNow.AddDays(-1));
        var last12Hours = seqEvents.Count(e => e.Timestamp > DateTime.UtcNow.AddHours(-12));
        var last6Hours = seqEvents.Count(e => e.Timestamp > DateTime.UtcNow.AddHours(-6));
        var lastHour = seqEvents.Count(e => e.Timestamp > DateTime.UtcNow.AddHours(-1));

        var signWeek = seqEvents.Count == lastWeek ? ">" : "";
        var sign24 = seqEvents.Count == last24Hours ? ">" : "";
        var sign12 = seqEvents.Count == last12Hours ? ">" : "";
        var sign6 = seqEvents.Count == last6Hours ? ">" : "";
        var sign1 = seqEvents.Count == lastHour ? ">" : "";
        return $"{signWeek}{lastWeek} events in last week{NewLine}" +
               $"{sign24}{last24Hours} events in 24 hours{NewLine}" +
               $"{sign12}{last12Hours} events in 12 hours{NewLine}" +
               $"{sign6}{last6Hours} events in 6 hours{NewLine}" +
               $"{sign1}{lastHour} events in last hour";
    }
}