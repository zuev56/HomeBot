﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HomeBot.Features.Hardware;
using HomeBot.Features.Interaction;
using HomeBot.Features.Ping;
using HomeBot.Features.Seq;
using HomeBot.Features.VkUsers;
using HomeBot.Features.Weather;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.PostgreSQL;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Scheduling;
using Zs.Common.Utilities;
using static HomeBot.Features.VkUsers.Constants;

namespace HomeBot;

internal sealed class HomeBot : IHostedService
{
    private readonly IScheduler _scheduler;
    private readonly HardwareMonitor _hardwareMonitor;
    private readonly UserWatcher _userWatcher;
    private readonly WeatherAnalyzer _weatherAnalyzer;
    private readonly SeqEventsInformer _seqEventsInformer;
    private readonly Notifier _notifier;
    private readonly PingChecker _pingChecker;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HomeBot> _logger;

    public HomeBot(
        IScheduler scheduler,
        HardwareMonitor hardwareMonitor,
        UserWatcher userWatcher,
        WeatherAnalyzer weatherAnalyzer,
        SeqEventsInformer seqEventsInformer,
        Notifier notifier,
        PingChecker pingChecker,
        IServiceProvider serviceProvider,
        ILogger<HomeBot> logger)
    {
        _scheduler = scheduler;
        _hardwareMonitor = hardwareMonitor;
        _userWatcher = userWatcher;
        _weatherAnalyzer = weatherAnalyzer;
        _seqEventsInformer = seqEventsInformer;
        _notifier = notifier;
        _pingChecker = pingChecker;
        _serviceProvider = serviceProvider;
        _logger = logger;

        CreateJobs();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await InitializeDataBaseAsync();
            _scheduler.Start(3.Seconds(), 1.Seconds());

            var startMessage = $"{nameof(HomeBot)} started."
                               + Environment.NewLine + Environment.NewLine
                               + RuntimeInformationWrapper.GetRuntimeInfo();
            await _notifier.ForceNotifyAsync(startMessage);

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
        _scheduler.Jobs.Add(_userWatcher.Job);
        _scheduler.Jobs.Add(_hardwareMonitor.Job);
        _scheduler.Jobs.Add(_weatherAnalyzer.Job);
        _scheduler.Jobs.Add(_seqEventsInformer.DayEventsInformerJob);
        _scheduler.Jobs.Add(_seqEventsInformer.NightEventsInformerJob);
        _scheduler.Jobs.Add(_pingChecker.Job);
        _scheduler.Jobs.Add(LogProcessStateJob());
        _scheduler.SetDefaultExecutionCompletedHandler<string>(Job_ExecutionCompleted);
    }

    private ProgramJob LogProcessStateJob()
    {
        var logProcessStateInfo = new ProgramJob(
            period: 1.Days(),
            method: () => Task.Run(() => _logger.LogProcessState(Process.GetCurrentProcess())),
            startUtcDate: DateTime.UtcNow + 1.Minutes(),
            description: "logProcessStateInfo"
        );
        return logProcessStateInfo;
    }

    private async void Job_ExecutionCompleted(Job<string> job, Result<string> result)
    {
        try
        {
            if (result.Successful == false)
            {
                _logger.LogWarning("Job \"{Job}\" execution failed {FaultCode}, {FaultMessage}", job.Description, result.Fault!.Code, result.Fault!.Message);
                return;
            }

            if (job.Description == InactiveUsersInformer)
            {
                await _notifier.NotifyOnceADayAsync(result.Value, "is not active for");
            }
            else
            {
                await _notifier.NotifyAsync(result.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job's ExecutionCompleted handler error");
        }
    }
}