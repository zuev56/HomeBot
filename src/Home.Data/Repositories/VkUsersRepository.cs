using Home.Data.Abstractions;
using Home.Data.Models.Vk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.Bot.Data.Repositories;

namespace Home.Data.Repositories
{
    public class VkUsersRepository<TContext> : CommonRepository<TContext, Data.Models.Vk.User, int>, IVkUsersRepository
        where TContext : DbContext
    {
        public VkUsersRepository(
            IDbContextFactory<TContext> contextFactory,
            TimeSpan? criticalQueryExecutionTimeForLogging = null,
            ILogger<CommonRepository<TContext, User, int>> logger = null)
            : base(contextFactory, criticalQueryExecutionTimeForLogging, logger)
        {
        }

        public async Task<List<User>> FindAllWhereNameLikeValueAsync(string value, int? skip, int? take)
        {
            return await FindAllAsync(u => EF.Functions.ILike(u.FirstName, $"%{value}%") || EF.Functions.ILike(u.LastName, $"%{value}%"), skip: skip, take: take);
        }
    }
}
