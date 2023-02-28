using System;
using System.Collections.Generic;
using Home.Bot.Abstractions;
using Home.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Home.Bot.Services;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Enums;
using Zs.Bot.Services.Messaging;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Logging.Seq;
using Zs.Common.Services.Scheduler;

namespace Home.Bot
{
    internal class HomeBot : IHostedService
    {
        private readonly IHardwareMonitor _hardwareMonitor;
        private readonly IUserWatcher _userWatcher;
        private readonly IConfiguration _configuration;
        private readonly IMessenger _messenger;
        private readonly IScheduler _scheduler;
        private readonly IMessagesRepository _messagesRepo;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISeqService? _seqService;
        private readonly WeatherAnalyzer _weatherAnalyzer;
        private readonly ILogger<HomeBot> _logger;

        public HomeBot(
            IConfiguration configuration,
            IMessenger messenger,
            IScheduler scheduler,
            IMessagesRepository messagesRepo,
            IHardwareMonitor hardwareMonitor,
            IUserWatcher userWatcher,
            IServiceProvider serviceProvider,
            WeatherAnalyzer weatherAnalyzer,
            ILogger<HomeBot> logger,
            ISeqService? seqService = null)
        {
            _configuration = configuration;
            _messenger = messenger;
            _scheduler = scheduler;
            _messagesRepo = messagesRepo;
            _hardwareMonitor = hardwareMonitor;
            _userWatcher = userWatcher;
            _serviceProvider = serviceProvider;
            _weatherAnalyzer = weatherAnalyzer;
            _logger = logger;
            _seqService = seqService;

            CreateJobs();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await InitializeDataBaseAsync();

                await _hardwareMonitor.StartAsync(cancellationToken);
                _scheduler.Start(3.Seconds(), 1.Seconds());

                string startMessage = $"Bot '{nameof(HomeBot)}' started."
                    + Environment.NewLine + Environment.NewLine
                    + RuntimeInformationWrapper.GetRuntimeInfo();
                await _messenger.AddMessageToOutboxAsync(startMessage, Role.Owner, Role.Admin);

                _logger?.LogInformation(startMessage);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Bot starting error");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _scheduler.Stop();
            await _hardwareMonitor.StopAsync(cancellationToken);

            _logger?.LogInformation("Bot stopped");
        }

        private async Task InitializeDataBaseAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<HomeContext>();

                await db.Database.EnsureCreatedAsync();
            }
        }

        private void CreateJobs()
        {
            _scheduler.Jobs.AddRange(_hardwareMonitor.Jobs);

            var inactiveUsersInformerJob = new ProgramJob<string>(
                period: TimeSpan.FromMinutes(5),
                method: _userWatcher.DetectLongTimeInactiveUsersAsync,
                description: Constants.InactiveUsersInformer,
                startUtcDate: DateTime.UtcNow + TimeSpan.FromSeconds(5),
                logger: _logger);

            inactiveUsersInformerJob.ExecutionCompleted += Job_ExecutionCompleted;
            _scheduler.Jobs.Add(inactiveUsersInformerJob);

            var hardwareMonitorInformerJob = (Job<string>)_scheduler.Jobs.Single(j => j.Description == Constants.HardwareWarningsInformer);
            hardwareMonitorInformerJob.ExecutionCompleted += Job_ExecutionCompleted;

            if (true)
            {
                var analyzeWeatherJob = new ProgramJob<string>(
                    period: TimeSpan.FromSeconds(300),
                    method: async () => await _weatherAnalyzer.AnalyzeAsync(),
                    startUtcDate: DateTime.UtcNow + TimeSpan.FromSeconds(10)
                );
                analyzeWeatherJob.ExecutionCompleted += Job_ExecutionCompleted;
                _scheduler.Jobs.Add(analyzeWeatherJob);
            }

            if (_seqService != null)
            {
                var dayErrorsAndWarningsInformer = new ProgramJob<string>(
                    period: TimeSpan.FromHours(1),
                    method: () => GetSeqEventsAsync(DateTime.UtcNow - TimeSpan.FromHours(1)),
                    startUtcDate: DateTime.UtcNow.NextHour(),
                    description: "dayErrorsAndWarningsInformer"
                );
                dayErrorsAndWarningsInformer.ExecutionCompleted += Job_ExecutionCompleted;
                _scheduler.Jobs.Add(dayErrorsAndWarningsInformer);

                var nightErrorsAndWarningsInformer = new ProgramJob<string>(
                    period: TimeSpan.FromDays(1),
                    method: () => GetSeqEventsAsync(DateTime.UtcNow - TimeSpan.FromHours(12)),
                    startUtcDate: DateTime.UtcNow.Date + TimeSpan.FromHours(24 + 10),
                    description: "nightErrorsAndWarningsInformer"
                );
                nightErrorsAndWarningsInformer.ExecutionCompleted += Job_ExecutionCompleted;
                _scheduler.Jobs.Add(nightErrorsAndWarningsInformer);
            }

            var logProcessStateInfo = new ProgramJob(
                period: TimeSpan.FromDays(1),
                method: () => Task.Run(() => _logger.LogProcessState(Process.GetCurrentProcess())),
                startUtcDate: DateTime.UtcNow + TimeSpan.FromMinutes(1),
                description: "logProcessStateInfo"
            );
            _scheduler.Jobs.Add(logProcessStateInfo);
        }

        private async Task<string> GetSeqEventsAsync(DateTime fromDate)
        {
            var events = await _seqService?.GetLastEventsAsync(fromDate, 10, _configuration.GetSection("Seq:ObservedSignals").Get<int[]>());
            return events.Count > 0
                ? string.Join(Environment.NewLine + Environment.NewLine, events)
                : string.Empty;
        }

        private string CreateMessageFromSeqEvents(List<SeqEvent> events)
        {
            var sb = new StringBuilder();

            foreach (var seqEvent in events)
            {
                sb.AppendFormat("[{0}]: ", seqEvent.Level.ToUpperInvariant()).Append(seqEvent.Date.AddHours(3)).AppendLine(); // Utc to Gmt+3
                sb.Append("Data:").AppendLine();
                seqEvent.Properties.ForEach(p => sb.Append("  • ").Append(p.ReplaceEndingWithThreeDots(150)).AppendLine());
                sb.Append("Messages:").AppendLine();
                seqEvent.Messages.ForEach(m => sb.Append("  • ").Append(m.ReplaceEndingWithThreeDots(300)).AppendLine());
                sb.Append(_seqService.Url).Append('/').Append(seqEvent.LinkPart.Substring(0, seqEvent.LinkPart.IndexOf('{'))).Append("?render");
                sb.AppendLine().AppendLine();
            }

            return sb.ToString();
        }



        private async void Job_ExecutionCompleted(Job<string> job, Result<string> result)
        {
            if (result?.Successful == false)
            {
                _logger.LogWarning("Job \"{Job}\" execution failed. Resuob, IOperationRelt: {Result}", job.Description, result.Value);
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

                    if (job.Description == Constants.InactiveUsersInformer)
                    {
                        var todaysAlerts = await _messagesRepo.FindAllTodaysMessagesWithTextAsync("is not active for");

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
                _logger?.LogError(ex, "Job's ExecutionCompleted handler error", result);
            }
        }
    }
}