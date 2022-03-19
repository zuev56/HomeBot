using Home.Data.Abstractions;
using Home.Data.Models.Vk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zs.Bot.Data.Repositories;
using Zs.Common.Extensions;

namespace Home.Data.Repositories
{

    public class ActivityLogItemsRepository<TContext> : CommonRepository<TContext, ActivityLogItem, int>, IActivityLogItemsRepository
        where TContext : DbContext
    {
        public ActivityLogItemsRepository(
            IDbContextFactory<TContext> contextFactory,
            TimeSpan? criticalQueryExecutionTimeForLogging = null,
            ILogger<CommonRepository<TContext, ActivityLogItem, int>> logger = null) 
            : base(contextFactory, criticalQueryExecutionTimeForLogging, logger)
        {
        }

        public async Task<List<ActivityLogItem>> FindAllByIdsInDateRangeAsync(int[] userIds, DateTime fromDate, DateTime toDate)
        {
            return await FindAllAsync(l => userIds.Contains(l.UserId)
               && l.LastSeen >= fromDate.ToUnixEpoch()
               && l.LastSeen <= toDate.ToUnixEpoch());
        }

        public async Task<List<ActivityLogItem>> FindLastUsersActivity(params int[] userIds)
        {
            var sql = @"WITH RECURSIVE t AS ( 
                          (SELECT * FROM vk.activity_log ORDER BY user_id DESC, last_seen DESC, insert_date DESC LIMIT 1)
                          UNION ALL SELECT bpt.* FROM t, 
                          LATERAL (SELECT * FROM vk.activity_log WHERE user_id < t.user_id ORDER BY user_id DESC, last_seen DESC, insert_date DESC LIMIT 1) AS bpt 
                        ) SELECT * FROM t";

            if (userIds?.Length > 0)
                sql += $" WHERE t.user_id in ({string.Join(',', userIds)})";
            
            return await FindAllBySqlAsync(sql);
        }

    }
}
