using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HomeBot.Features.Hardware;
using HomeBot.Features.Interaction;
using HomeBot.Features.Ping;
using HomeBot.Features.Seq;
using HomeBot.Features.VkUsers;
using HomeBot.Features.Weather;
using HomeBot.MessagePipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.Messaging;
using Zs.Bot.Services.Pipeline;
using Zs.Bot.Services.Storages;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Scheduling;
using Zs.Common.Utilities;
using static HomeBot.Features.VkUsers.Constants;

namespace HomeBot;

internal sealed class HomeBot : IHostedService
{
    private readonly IBotClient _botClient;
    private readonly IMessageDataStorage _messageDataStorage;
    private readonly IScheduler _scheduler;
    private readonly Notifier _notifier;
    private readonly BotSettings _botSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HomeBot> _logger;

    public HomeBot(
        IBotClient botClient,
        IMessageDataStorage messageDataStorage,
        IScheduler scheduler,
        Notifier notifier,
        IOptions<BotSettings> botOptions,
        IServiceProvider serviceProvider,
        ILogger<HomeBot> logger)
    {
        _botClient = botClient;
        _messageDataStorage = messageDataStorage;
        _scheduler = scheduler;
        _notifier = notifier;
        _botSettings = botOptions.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;

        SetupMessagePipeline();
        CreateJobs();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
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

    private void SetupMessagePipeline()
    {
        // TODO: Вынести из этого класса
        var authorization = new Authorization(_botClient, _botSettings.PrivilegedUserRawIds);
        var localMessageHandler = new MessageHandler(_serviceProvider.GetRequiredService<CommandHandler>());
        var commandManager = _serviceProvider.GetRequiredService<ICommandManager>();

        _botClient
            .UseLogger(_logger)
            .UseMessageDataSaver(_messageDataStorage)
            .Use(authorization)
            .Use(localMessageHandler)
            .UseCommandManager(commandManager, BotSettings.GetMessageText, _botSettings.Name);
    }

    private void CreateJobs()
    {
        // TODO: Вынести из этого класса
        var userWatcher = _serviceProvider.GetRequiredService<UserWatcher>();
        var hardwareMonitor = _serviceProvider.GetRequiredService<HardwareMonitor>();
        var weatherAnalyzer = _serviceProvider.GetRequiredService<WeatherAnalyzer>();
        var seqEventsInformer = _serviceProvider.GetRequiredService<SeqEventsInformer>();
        var pingChecker = _serviceProvider.GetRequiredService<PingChecker>();

        _scheduler.Jobs.Add(userWatcher.Job);
        _scheduler.Jobs.Add(hardwareMonitor.Job);
        _scheduler.Jobs.Add(weatherAnalyzer.Job);
        _scheduler.Jobs.Add(seqEventsInformer.DayEventsInformerJob);
        _scheduler.Jobs.Add(seqEventsInformer.NightEventsInformerJob);
        _scheduler.Jobs.Add(pingChecker.Job);
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