using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeBot.Features.HardwareMonitor;
using HomeBot.Features.Seq;
using HomeBot.Features.UserWatcher;
using HomeBot.Features.WeatherAnalyzer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Enums;
using Zs.Bot.Data.PostgreSQL;
using Zs.Bot.Services.Messaging;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Scheduling;
using Zs.Common.Utilities;
using static HomeBot.Features.UserWatcher.Constants;

namespace HomeBot;

internal sealed class HomeBot : IHostedService
{
    private readonly HardwareMonitor _hardwareMonitor;
    private readonly UserWatcher _userWatcher;
    private readonly IConfiguration _configuration;
    private readonly IMessenger _messenger;
    private readonly IScheduler _scheduler;
    private readonly IMessagesRepository _messagesRepo;
    private readonly IServiceProvider _serviceProvider;
    private readonly SeqEventsInformer _seqEventsInformer;
    private readonly WeatherAnalyzer _weatherAnalyzer;
    private readonly ILogger<HomeBot> _logger;

    public HomeBot(
        IConfiguration configuration,
        IMessenger messenger,
        IScheduler scheduler,
        IMessagesRepository messagesRepo,
        HardwareMonitor hardwareMonitor,
        UserWatcher userWatcher,
        IServiceProvider serviceProvider,
        WeatherAnalyzer weatherAnalyzer,
        SeqEventsInformer seqEventsInformer,
        ILogger<HomeBot> logger)
    {
        _configuration = configuration;
        _messenger = messenger;
        _scheduler = scheduler;
        _messagesRepo = messagesRepo;
        _hardwareMonitor = hardwareMonitor;
        _userWatcher = userWatcher;
        _serviceProvider = serviceProvider;
        _weatherAnalyzer = weatherAnalyzer;
        _seqEventsInformer = seqEventsInformer;
        _logger = logger;

        CreateJobs();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await InitializeDataBaseAsync();

            _hardwareMonitor.Start();
            _scheduler.Start(3.Seconds(), 1.Seconds());

            var startMessage = $"{nameof(HomeBot)} started."
                               + Environment.NewLine + Environment.NewLine
                               + RuntimeInformationWrapper.GetRuntimeInfo();
            await _messenger.AddMessageToOutboxAsync(startMessage, Role.Owner, Role.Admin);

            _logger.LogInformation(startMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bot starting error");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _scheduler.Stop();
        _hardwareMonitor.Stop();

        _logger.LogInformation("Bot stopped");

        return Task.CompletedTask;
    }

    private async Task InitializeDataBaseAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<PostgreSqlBotContext>();

        await db.Database.EnsureCreatedAsync();
    }

    private void CreateJobs()
    {
        _userWatcher.Job.ExecutionCompleted += Job_ExecutionCompleted;
        _scheduler.Jobs.Add(_userWatcher.Job);

        _hardwareMonitor.Job.ExecutionCompleted += Job_ExecutionCompleted;
        _scheduler.Jobs.Add(_hardwareMonitor.Job);

        _weatherAnalyzer.Job.ExecutionCompleted += Job_ExecutionCompleted;
        _scheduler.Jobs.Add(_weatherAnalyzer.Job);

        _seqEventsInformer.DayEventsInformerJob.ExecutionCompleted += Job_ExecutionCompleted;
        _scheduler.Jobs.Add(_seqEventsInformer.DayEventsInformerJob);

        _seqEventsInformer.NightEventsInformerJob.ExecutionCompleted += Job_ExecutionCompleted;
        _scheduler.Jobs.Add(_seqEventsInformer.NightEventsInformerJob);

        var logProcessStateInfo = new ProgramJob(
            period: 1.Days(),
            method: () => Task.Run(() => _logger.LogProcessState(Process.GetCurrentProcess())),
            startUtcDate: DateTime.UtcNow + 1.Minutes(),
            description: "logProcessStateInfo"
        );
        _scheduler.Jobs.Add(logProcessStateInfo);
    }

    private async void Job_ExecutionCompleted(Job<string> job, Result<string>? result)
    {
        if (result?.Successful == false)
        {
            _logger.LogWarning("Job \"{Job}\" execution failed", job.Description);
        }

        // On start
        if (result == null)
        {
            return;
        }

        try
        {
            if (result.Successful && !string.IsNullOrWhiteSpace(result.Value)
                                  && DateTime.Now.Hour >= _configuration.GetValue<int>("Notifier:Time:FromHour")
                                  && DateTime.Now.Hour < _configuration.GetValue<int>("Notifier:Time:ToHour"))
            {
                var preparedMessage = result.Value.ReplaceEndingWithThreeDots(4000);

                if (job.Description == InactiveUsersInformer)
                {
                    var todaysAlerts = await _messagesRepo.FindAllTodayMessagesWithTextAsync("is not active for");

                    if (!todaysAlerts.Any(m => m.Text?.WithoutDigits() == preparedMessage.WithoutDigits()))
                    {
                        await _messenger.AddMessageToOutboxAsync(preparedMessage, Role.Owner, Role.Admin);
                    }
                }
                else
                {
                    await _messenger.AddMessageToOutboxAsync(preparedMessage, Role.Owner, Role.Admin);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job's ExecutionCompleted handler error", result);
        }
    }
}