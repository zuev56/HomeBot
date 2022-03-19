using System;
using System.Threading.Tasks;
using Home.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Zs.Bot.Data.PostgreSQL;

namespace Home.Service.Tests.Data;

public class PostgreSqlInMemory
{
    public ActivityLogItemsRepository<PostgreSqlBotContext> ActivityLogItemsRepository { get; }
    public VkUsersRepository<PostgreSqlBotContext> VkUsersRepository { get; }

    public PostgreSqlInMemory()
    {
        var dbContextFactory = GetPostgreSqlBotContextFactory();

        ActivityLogItemsRepository = new ActivityLogItemsRepository<PostgreSqlBotContext>(dbContextFactory);
        VkUsersRepository = new VkUsersRepository<PostgreSqlBotContext>(dbContextFactory);
    }

    private PostgreSqlBotContextFactory GetPostgreSqlBotContextFactory()
    {
        var dbName = $"PostgreSQLInMemoryDB_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<PostgreSqlBotContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new PostgreSqlBotContextFactory(options);
    }

    public void FillWithFakeData(int entitiesCount)
    {
        var activityLogItems = StubFactory.CreateActivityLogItems(entitiesCount);
        var vkUsers = StubFactory.CreateVkUsers(entitiesCount);


        Task.WaitAll(new Task[]
        {
            ActivityLogItemsRepository.SaveRangeAsync(activityLogItems),
            VkUsersRepository.SaveRangeAsync(vkUsers),
        });
    }
}