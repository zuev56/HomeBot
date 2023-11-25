using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Common.Services.Logging.Seq;
using Zs.Common.Services.Scheduling;

namespace HomeBot.Features.Seq;

public sealed class SeqEventsInformer
{
    private readonly SeqSettings _options;
    private readonly ISeqService _seqService;
    public ProgramJob<string> DayEventsInformerJob { get; }
    public ProgramJob<string> NightEventsInformerJob { get; }

    public SeqEventsInformer(
        IOptions<SeqSettings> options,
        ISeqService seqService,
        ILogger<SeqEventsInformer> logger)
    {
        _options = options.Value;
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
        var events = await _seqService.GetLastEventsAsync(fromDate, 10, _options.ObservedSignals);

        return events.Count > 0
            ? CreateMessageFromSeqEvents(events)
            : string.Empty;
    }

    private string CreateMessageFromSeqEvents(IEnumerable<SeqEvent> events)
    {
        var sb = new StringBuilder();

        foreach (var seqEvent in events)
        {
            var level = seqEvent.Level.ToUpperInvariant();
            var localDate = seqEvent.Date.AddHours(3).ToString("dd.MM.yyyy HH:mm:ss");

            sb.AppendFormat("[{0}]: ", level).Append(localDate).AppendLine();

            sb.Append("Properties:").AppendLine();
            seqEvent.Properties
                .ForEach(p => sb.Append("  • ").Append(p.ReplaceEndingWithThreeDots(150)).AppendLine());

            sb.Append("Messages:").AppendLine();
            seqEvent.Messages
                .ForEach(m => sb.Append("  • ").Append(m.ReplaceEndingWithThreeDots(300)).AppendLine());

            sb.Append(_seqService.Url).Append('/').Append(seqEvent.LinkPart[..seqEvent.LinkPart.IndexOf('{')]).Append("?render");
            sb.AppendLine().AppendLine();
        }

        return sb.ToString();
    }
}