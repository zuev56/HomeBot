using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Home.Bot.Abstractions;
using Home.Bot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.DataSavers;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Scheduler;

namespace Home.Bot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(CreateConfiguration(args), "Serilog")
                    .CreateLogger();

                Log.Warning("-! Starting {ProcessName} (MachineName: {MachineName}, OS: {OS}, User: {User}, ProcessId: {ProcessId})",
                    Process.GetCurrentProcess().MainModule!.ModuleName,
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

            return configuration;
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.AddConfiguration(CreateConfiguration(args)))
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    services
                        .AddDatabase(configuration)
                        .AddConnectionAnalyzer(configuration)
                        .AddTelegramBot(configuration)
                        .AddSeq(configuration)
                        .AddDbClient(configuration)
                        .AddShellLauncher(configuration);

                    services.AddScoped<IScheduler, Scheduler>();
                    services.AddScoped<IMessageDataSaver, MessageDataDBSaver>();
                    services.AddScoped<ICommandManager, CommandManager>();
                    services.AddScoped<IHardwareMonitor, ThinkPadX230HardwareMonitor>();
                    services.AddScoped<IUserWatcher, UserWatcher>();

                    services.AddHostedService<HomeBot>();
                });
        }
    }
}