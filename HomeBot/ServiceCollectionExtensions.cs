using System.Net.Http;
using HomeBot.Features.Hardware;
using HomeBot.Features.Interaction;
using HomeBot.Features.Ping;
using HomeBot.Features.Seq;
using HomeBot.Features.VkUsers;
using HomeBot.Features.Weather;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
using Zs.Common.Services.Logging.Seq;
using Zs.EspMeteo.Parser;

namespace HomeBot;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings:Default"];
        services.AddDbContextFactory<PostgreSqlBotContext>(options => options.UseNpgsql(connectionString));

        services.AddSingleton<ICommandsRepository, CommandsRepository<PostgreSqlBotContext>>();
        services.AddSingleton<IUserRolesRepository, UserRolesRepository<PostgreSqlBotContext>>();
        services.AddSingleton<IChatsRepository, ChatsRepository<PostgreSqlBotContext>>();
        services.AddSingleton<IUsersRepository, UsersRepository<PostgreSqlBotContext>>();
        services.AddSingleton<IMessagesRepository, MessagesRepository<PostgreSqlBotContext>>();

        return services;
    }

    internal static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        var token = configuration["Bot:Token"]!;
        var httpClient = new HttpClient();
        var telegramBotClient = new TelegramBotClient(token, httpClient);

        services.AddSingleton<ITelegramBotClient>(telegramBotClient);
        services.AddSingleton<IMessenger, TelegramMessenger>();
        services.AddSingleton<IMessageDataSaver, MessageDataDbSaver>();

        return services;
    }

    internal static IServiceCollection AddSeq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SeqOptions>()
            .Bind(configuration.GetSection(SeqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ISeqService, SeqService>(static provider =>
        {
            var options = provider.GetRequiredService<IOptions<SeqOptions>>().Value;
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
        services.AddOptions<WeatherAnalyzerOptions>()
            .Bind(configuration.GetSection(WeatherAnalyzerOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<EspMeteoParser>();

        services.AddSingleton<WeatherAnalyzer>();

        return services;
    }

    internal static IServiceCollection AddCommandManager(this IServiceCollection services)
    {
        services.AddSingleton<ICommandManager, CommandManager>(provider =>
        {
            var commandsRepository = provider.GetRequiredService<ICommandsRepository>();
            var userRolesRepository = provider.GetRequiredService<IUserRolesRepository>();
            var usersRepository = provider.GetRequiredService<IUsersRepository>();
            var dbClient = provider.GetRequiredService<IDbClient>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var bashPath = configuration["Bot:BashPath"];
            var powershellPath = configuration["Bot:PowerShellPath"];
            var logger = provider.GetRequiredService<ILogger<CommandManager>>();

            return new CommandManager(
                commandsRepository,
                userRolesRepository,
                usersRepository,
                dbClient,
                bashPath,
                powershellPath,
                logger
            );
        });

        return services;
    }

    internal static IServiceCollection AddUserWatcher(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<UserWatcherOptions>()
            .Bind(configuration.GetSection(UserWatcherOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<UserWatcher>();

        return services;
    }

    internal static IServiceCollection AddHardwareMonitor(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<HardwareMonitorOptions>()
            .Bind(configuration.GetSection(HardwareMonitorOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<HardwareMonitor, LinuxHardwareMonitor>();

        return services;
    }

    internal static IServiceCollection AddInteractionServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NotifierOptions>()
            .Bind(configuration.GetSection(NotifierOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<Notifier>();
        services.AddSingleton<SystemStatusService>();

        return services;
    }

    internal static IServiceCollection AddPingChecker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PingCheckerOptions>()
            .Bind(configuration.GetSection(PingCheckerOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<PingChecker>();

        return services;
    }

}