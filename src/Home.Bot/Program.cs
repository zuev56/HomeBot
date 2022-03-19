using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Home.Bot.Abstractions;
using Home.Bot.Services;
using Home.Data;
using Home.Data.Abstractions;
using Home.Data.Repositories;
using Home.Services.Vk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Telegram.Bot;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.PostgreSQL;
using Zs.Bot.Data.PostgreSQL.Repositories;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Messenger.Telegram;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.DataSavers;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Logging.Seq;
using Zs.Common.Services.Scheduler;
using Zs.Common.Services.Shell;

namespace Home.Bot
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                Serilog.Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(CreateConfiguration(args), "Serilog")
                    .CreateLogger();

                Log.Warning("-! Starting {ProcessName} (MachineName: {MachineName}, OS: {OS}, User: {User}, ProcessId: {ProcessId})",
                    Process.GetCurrentProcess().MainModule.ModuleName,
                    Environment.MachineName,
                    Environment.OSVersion,
                    Environment.UserName,
                    Environment.ProcessId);

                await CreateHostBuilder(args).RunConsoleAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ProgramUtilites.TrySaveFailInfo(ex.ToText());
                Console.WriteLine(ex.ToText());
                Console.Read();
            }
        }

        private static IConfiguration CreateConfiguration(string[] args)
        {
            if (!File.Exists(ProgramUtilites.MainConfigurationPath))
                throw new AppsettingsNotFoundException();

            var configuration = new ConfigurationManager();
            configuration.AddJsonFile(ProgramUtilites.MainConfigurationPath, optional: false, reloadOnChange: true);

            foreach (var arg in args)
            {
                if (!File.Exists(arg))
                    throw new FileNotFoundException($"Wrong configuration path:\n{arg}");

                configuration.AddJsonFile(arg, optional: true, reloadOnChange: true);
            }

            if (configuration["SecretsPath"] != null)
                configuration.AddJsonFile(configuration["SecretsPath"]);

            AssertConfigurationIsCorrect(configuration);

            return configuration;
        }

        private static void AssertConfigurationIsCorrect(IConfiguration configuration)
        {
            // TODO
            // BotToken
            // ConnectionStrings
            // etc
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, configurationBuilder) => configurationBuilder.AddConfiguration(CreateConfiguration(args)))
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<HomeContext>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetSecretValue("ConnectionStrings:Default")));

                    services.AddDbContext<PostgreSqlBotContext>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetSecretValue("ConnectionStrings:Default")));
                    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

                    // For repositories
                    services.AddScoped<IDbContextFactory<HomeContext>, HomeContextFactory>();
                    services.AddScoped<IDbContextFactory<PostgreSqlBotContext>, PostgreSqlBotContextFactory>();

                    services.AddScoped<IConnectionAnalyser, ConnectionAnalyser>(sp =>
                    {
                        var connectionAnalyzer = new ConnectionAnalyser(
                            sp.GetService<ILogger<ConnectionAnalyser>>(),
                            hostContext.Configuration.GetSection("ConnectionAnalyser:Urls").Get<string[]>());

                        if (hostContext.Configuration.GetSection("Proxy:UseProxy")?.Get<bool>() == true)
                        {
                            connectionAnalyzer.InitializeProxy(hostContext.Configuration["Proxy:Socket"],
                                hostContext.Configuration.GetSecretValue("Proxy:Login"),
                                hostContext.Configuration.GetSecretValue("Proxy:Password"));

                            HttpClient.DefaultProxy = connectionAnalyzer.WebProxy;
                        }
                        return connectionAnalyzer;
                    });

                    services.AddScoped<ISeqService, SeqService>(sp =>
                        new SeqService(hostContext.Configuration["Seq:ServerUrl"], hostContext.Configuration.GetSecretValue("Seq:ApiToken")));

                    services.AddScoped<ITelegramBotClient>(sp =>
                        new TelegramBotClient(hostContext.Configuration.GetSecretValue("Bot:Token"), new HttpClient()));

                    services.AddScoped<IMessenger, TelegramMessenger>();

                    services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository<HomeContext>>();
                    services.AddScoped<IVkUsersRepository, VkUsersRepository<HomeContext>>();
                    services.AddScoped<ICommandsRepository, CommandsRepository<PostgreSqlBotContext>>();
                    services.AddScoped<IUserRolesRepository, UserRolesRepository<PostgreSqlBotContext>>();
                    services.AddScoped<IChatsRepository, ChatsRepository<PostgreSqlBotContext>>();
                    services.AddScoped<IUsersRepository, UsersRepository<PostgreSqlBotContext>>();
                    services.AddScoped<IMessagesRepository, MessagesRepository<PostgreSqlBotContext>>();

                    services.AddScoped<IScheduler, Scheduler>();
                    services.AddScoped<IActivityLoggerService, ActivityLoggerService>();
                    services.AddScoped<IMessageDataSaver, MessageDataDBSaver>();
                    services.AddScoped<IDbClient, DbClient>(sp =>
                        new DbClient(
                            hostContext.Configuration.GetSecretValue("ConnectionStrings:Default"),
                            sp.GetService<ILogger<DbClient>>())
                        );

                    services.AddScoped<IShellLauncher, ShellLauncher>(sp =>
                    new ShellLauncher(
                        bashPath: hostContext.Configuration.GetSecretValue("Bot:BashPath"),
                        powerShellPath: hostContext.Configuration.GetSecretValue("Bot:PowerShellPath")
                    ));
                    services.AddScoped<ICommandManager, CommandManager>();
                    services.AddScoped<IHardwareMonitor, ThinkPadX230HardwareMonitor>();
                    services.AddScoped<IUserWatcher, Services.UserWatcher>();

                    services.AddHostedService<HomeBot>();
                });
        }

        
    }
}
