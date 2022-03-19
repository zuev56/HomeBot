using Home.Data.Models.Vk;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.Bot.Data.Abstractions;

namespace Home.Data.Abstractions
{
    public interface IActivityLogItemsRepository : IRepository<ActivityLogItem, int>
    {
        Task<List<ActivityLogItem>> FindLastUsersActivity(params int[] userIds);
        Task<List<ActivityLogItem>> FindAllByIdsInDateRangeAsync(int[] userIds, DateTime fromDate, DateTime toDate);
    }
}
