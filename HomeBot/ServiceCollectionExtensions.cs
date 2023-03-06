using System.Net.Http;
using HomeBot.Features.HardwareMonitor;
using HomeBot.Features.UserWatcher;
using HomeBot.Features.WeatherAnalyzer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        services.AddDbContext<PostgreSqlBotContext>(options => options.UseNpgsql(connectionString));

        // For repositories
        services.AddScoped<IDbContextFactory<PostgreSqlBotContext>, PostgreSqlBotContextFactory>();

        services.AddScoped<ICommandsRepository, CommandsRepository<PostgreSqlBotContext>>();
        services.AddScoped<IUserRolesRepository, UserRolesRepository<PostgreSqlBotContext>>();
        services.AddScoped<IChatsRepository, ChatsRepository<PostgreSqlBotContext>>();
        services.AddScoped<IUsersRepository, UsersRepository<PostgreSqlBotContext>>();
        services.AddScoped<IMessagesRepository, MessagesRepository<PostgreSqlBotContext>>();

        return services;
    }

    // TODO: move to Zs.Common.Services

    internal static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        var token = configuration["Bot:Token"];
        var httpClient = new HttpClient();

        services.AddScoped<ITelegramBotClient>(_ => new TelegramBotClient(token, httpClient));
        services.AddScoped<IMessenger, TelegramMessenger>();
        services.AddScoped<IMessageDataSaver, MessageDataDbSaver>();

        return services;
    }

    internal static IServiceCollection AddSeq(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration["Seq:ServerUrl"];
        var token = configuration["Seq:ApiToken"];

        services.AddScoped<ISeqService, SeqService>(_ => new SeqService(url, token));

        return services;
    }

    internal static IServiceCollection AddDbClient(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings:Default"]!;

        services.AddScoped<IDbClient, DbClient>(sp =>
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
        //(static provider =>
        //{
        //    var options = provider.GetRequiredService<IOptions<WeatherAnalyzerOptions>>().Value;
        //    var parser = provider.GetRequiredService<EspMeteoParser>();
        //    var logger = provider.GetRequiredService<ILogger<WeatherAnalyzer>>();
        //    return new WeatherAnalyzer(parser, options, logger);
        //});

        return services;
    }

    internal static IServiceCollection AddCommandManager(this IServiceCollection services)
    {
        services.AddScoped<ICommandManager, CommandManager>(provider =>
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

        services.AddScoped<HardwareMonitor, LinuxHardwareMonitor>();

        return services;
    }
}