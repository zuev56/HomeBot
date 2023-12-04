using System;
using System.Collections.Generic;
using System.Linq;
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
        var seqEvents = await _seqService.GetLastEventsAsync(100, _settings.ObservedSignals)
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

            messageBuilder.AppendLine(localDate)
                .AppendLine($"App: {applicationName}")
                .Append(seqEvent.Level.ToUpperInvariant()).Append(": ").Append(seqEvent.Message);

            messageBuilder.AppendLine().AppendLine();
        }

        return messageBuilder.ToString().ReplaceEndingWithThreeDots(maxStringLength: 2000);
    }
}