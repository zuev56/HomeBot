using HomeBot.Features.Hardware;
using HomeBot.Features.Ping;
using HomeBot.Features.Seq;
using HomeBot.Features.VkUsers;
using HomeBot.Features.Weather;
using HomeBot.Interaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Bot.Data.PostgreSQL;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Services;
using Zs.Bot.Telegram.Extensions;
using Zs.Common.Abstractions;
using Zs.Common.Services.Logging.Seq;
using Zs.EspMeteo.Parser;

namespace HomeBot;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings:Default"];
        services.AddDbContextFactory<PostgreSqlBotContext>(options => options.UseNpgsql(connectionString));

        services.AddSingleton<IChatsRepository, ChatsRepository<PostgreSqlBotContext>>();
        services.AddSingleton<IUsersRepository, UsersRepository<PostgreSqlBotContext>>();
        services.AddSingleton<IMessagesRepository, MessagesRepository<PostgreSqlBotContext>>();

        return services;
    }

    internal static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BotSettings>()
            .Bind(configuration.GetSection(BotSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var settings = services.BuildServiceProvider().GetRequiredService<IOptions<BotSettings>>().Value;
        services.AddTelegramBotClient(settings.Token);
        services.AddPostgreSqlMessageDataStorage();
        services.AddCommandManager(settings.CliPath);

        return services;
    }

    internal static IServiceCollection AddSeq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SeqSettings2>()
            .Bind(configuration.GetSection(SeqSettings2.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ISeqService, SeqService>(static provider =>
        {
            var options = provider.GetRequiredService<IOptions<SeqSettings2>>().Value;
            var logger = provider.GetRequiredService<ILogger<SeqService>>();

            return new SeqService(options.Url, options.Token, logger);
        });

        services.AddSingleton<SeqEventsInformer>();

        return services;
    }

    internal static IServiceCollection AddDbClient(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings:Default"]!;

        services.AddSingleton<IDbClient, DbClient>(sp =>
            new DbClient(connectionString, sp.GetService<ILogger<DbClient>>()));

        return services;
    }

    internal static IServiceCollection AddWeatherAnalyzer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WeatherAnalyzerSettings>()
            .Bind(configuration.GetSection(WeatherAnalyzerSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<EspMeteoParser>();

        services.AddSingleton<WeatherAnalyzer>();

        return services;
    }

    internal static IServiceCollection AddUserWatcher(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<UserWatcherSettings>()
            .Bind(configuration.GetSection(UserWatcherSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<UserWatcher>();

        return services;
    }

    internal static IServiceCollection AddHardwareMonitor(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<HardwareMonitorSettings>()
            .Bind(configuration.GetSection(HardwareMonitorSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<HardwareMonitor, LinuxHardwareMonitor>();

        return services;
    }

    internal static IServiceCollection AddInteractionServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NotifierSettings>()
            .Bind(configuration.GetSection(NotifierSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<Notifier>();
        services.AddSingleton<SystemStatusService>();
        services.AddSingleton<CommandHandler>();

        return services;
    }

    internal static IServiceCollection AddPingChecker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PingCheckerSettings>()
            .Bind(configuration.GetSection(PingCheckerSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<PingChecker>();

        return services;
    }
}