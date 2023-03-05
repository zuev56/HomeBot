using System;
using System.Diagnostics;
using System.Globalization;
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
using Zs.Common.Services.Connection;
using Zs.Common.Services.Scheduling;

namespace Home.Bot;

public sealed class Program
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
            TrySaveFailInfo(ex.ToText());
            Console.WriteLine(ex.ToText());
            Console.Read();
        }
    }

    private static IConfiguration CreateConfiguration(string[] args)
    {
        var appsettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        if (!File.Exists(appsettingsPath))
            throw new AppsettingsNotFoundException();

        var configuration = new ConfigurationManager();
        configuration.AddJsonFile(appsettingsPath, optional: false, reloadOnChange: true);

        // foreach (var arg in args)
        // {
        //     if (!File.Exists(arg))
        //         throw new FileNotFoundException($"Wrong configuration path:\n{arg}");
        //
        //     configuration.AddJsonFile(arg, optional: true, reloadOnChange: true);
        // }
        //
        // if (configuration["SecretsPath"] != null)
        //     configuration.AddJsonFile(configuration["SecretsPath"]);

        return configuration;
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var cultureInfo = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.AddConfiguration(CreateConfiguration(args)))
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;

                services
                    .AddDatabase(configuration)
                    .AddConnectionAnalyzer()
                    .AddTelegramBot(configuration)
                    .AddSeq(configuration)
                    .AddDbClient(configuration)
                    .AddWeatherAnalyzer(configuration)
                    .AddCommandManager();

                services.AddScoped<IScheduler, Scheduler>();
                services.AddScoped<IMessageDataSaver, MessageDataDbSaver>();
                services.AddScoped<IHardwareMonitor, ThinkPadX230HardwareMonitor>();
                services.AddScoped<IUserWatcher, UserWatcher>();

                services.AddHostedService<HomeBot>();
            });
    }

    private static void TrySaveFailInfo(string text)
    {
        try
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), $"Critical_failure_{DateTime.Now:yyyy.MM.dd HH:mm:ss.ff}.log");
            File.AppendAllText(path, text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n{ex}\nMessage:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}");
        }
    }
}