using System.Net.Http;
using Home.Data;
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
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Logging.Seq;
using Zs.Common.Services.Shell;

namespace Home.Bot;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings:Default"];

        services.AddDbContext<HomeContext>(options => options.UseNpgsql(connectionString));

        services.AddDbContext<PostgreSqlBotContext>(options => options.UseNpgsql(connectionString));

        // For repositories
        services.AddScoped<IDbContextFactory<HomeContext>, HomeContextFactory>();
        services.AddScoped<IDbContextFactory<PostgreSqlBotContext>, PostgreSqlBotContextFactory>();

        services.AddScoped<ICommandsRepository, CommandsRepository<PostgreSqlBotContext>>();
        services.AddScoped<IUserRolesRepository, UserRolesRepository<PostgreSqlBotContext>>();
        services.AddScoped<IChatsRepository, ChatsRepository<PostgreSqlBotContext>>();
        services.AddScoped<IUsersRepository, UsersRepository<PostgreSqlBotContext>>();
        services.AddScoped<IMessagesRepository, MessagesRepository<PostgreSqlBotContext>>();

        return services;
    }

    // TODO: move to Zs.Common.Services
    internal static IServiceCollection AddConnectionAnalyzer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IConnectionAnalyser, ConnectionAnalyser>(sp =>
        {
            var urls = configuration.GetSection("ConnectionAnalyser:Urls").Get<string[]>();
            var logger = sp.GetService<ILogger<ConnectionAnalyser>>();
            var useProxy = configuration.GetSection("Proxy:UseProxy")?.Get<bool>() ?? false;

            var connectionAnalyzer = new ConnectionAnalyser(logger, urls);

            if (useProxy)
            {
                var socket = configuration["Proxy:Socket"];
                var login = configuration["Proxy:Login"];
                var password = configuration["Proxy:Password"];

                connectionAnalyzer.InitializeProxy(socket, login, password);
                HttpClient.DefaultProxy = connectionAnalyzer.WebProxy;
            }

            return connectionAnalyzer;
        });

        return services;
    }

    internal static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        var token = configuration["Bot:Token"];
        var httpClient = new HttpClient();

        services.AddScoped<ITelegramBotClient>(_ => new TelegramBotClient(token, httpClient));
        services.AddScoped<IMessenger, TelegramMessenger>();

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
        var connectionString = configuration["ConnectionStrings:Default"];

        services.AddScoped<IDbClient, DbClient>(sp =>
            new DbClient(connectionString, sp.GetService<ILogger<DbClient>>()));

        return services;
    }

    internal static IServiceCollection AddShellLauncher(this IServiceCollection services, IConfiguration configuration)
    {
        var bashPath = configuration["Bot:BashPath"];
        var powerShellPath = configuration["Bot:PowerShellPath"];

        services.AddScoped<IShellLauncher, ShellLauncher>(sp => new ShellLauncher(bashPath, powerShellPath));

        return services;
    }
}