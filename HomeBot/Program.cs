using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Zs.Bot.Data.PostgreSQL;
using Zs.Common.Extensions;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Scheduling;

namespace HomeBot;

public static class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            InitializeLogger();

            var hostBuilder = CreateHostBuilder(args);
            var host = hostBuilder.UseConsoleLifetime().Build();
            await InitializeDataBaseAsync(host.Services);
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            TrySaveFailInfo(ex.ToText());
            Console.WriteLine(ex.ToText());
            Console.Read();
        }
    }

    private static void InitializeLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(CreateConfiguration(), "Serilog")
            .CreateLogger();

        Log.Warning("-! Starting {ProcessName} (MachineName: {MachineName}, OS: {OS}, User: {User}, ProcessId: {ProcessId})",
            Process.GetCurrentProcess().MainModule!.ModuleName,
            Environment.MachineName,
            Environment.OSVersion,
            Environment.UserName,
            Environment.ProcessId);
    }

    private static IConfiguration CreateConfiguration()
    {
        var appsettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        if (!File.Exists(appsettingsPath))
            throw new InvalidOperationException("appsettings.json not found");

        var configuration = new ConfigurationManager();
        configuration.AddJsonFile(appsettingsPath, optional: false, reloadOnChange: true);

        return configuration;
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var cultureInfo = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        return Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices(static (hostContext, services) =>
            {
                var configuration = hostContext.Configuration;

                services
                    .AddDatabase(configuration)
                    .AddConnectionAnalyzer()
                    .AddTelegramBot(configuration)
                    .AddSeq(configuration)
                    .AddDbClient(configuration)
                    .AddWeatherAnalyzer(configuration)
                    .AddUserWatcher(configuration)
                    .AddHardwareMonitor(configuration)
                    .AddInteractionServices(configuration)
                    .AddPingChecker(configuration)
                    .AddSingleton<IScheduler, Scheduler>();

                services.AddHostedService<HomeBot>();
            });
    }

    private static async Task InitializeDataBaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<PostgreSqlBotContext>();

        await db.Database.EnsureCreatedAsync();
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