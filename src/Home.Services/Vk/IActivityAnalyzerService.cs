using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Home.Data.Models.Vk;
using Home.Services.Vk.Models;
using Zs.Common.Models;

namespace Home.Services.Vk;

public interface IActivityAnalyzerService
{
    /// <summary>Get information about user activity in the specified period</summary>
    Task<Result<SimpleUserActivity>> GetUserStatisticsForPeriodAsync(int userId, DateTime fromDate, DateTime toDate);

    /// <summary>Get detailed information about full user activity</summary>
    Task<Result<DetailedUserActivity>> GetFullTimeUserStatisticsAsync(int userId);

    /// <summary>Get users list </summary>
    Task<Result<List<User>>> GetUsersAsync(string filterText = null, int? skip = null, int? take = null);

    /// <summary>Get users list with activity time in a specified period</summary>
    Task<Result<List<UserWithActivity>>> GetUsersWithActivityAsync(string filterText, DateTime fromDate, DateTime toDate);

    ///// <summary> Get paginated users list with activity time in a specified period </summary>
    ///// <param name="filterText">Filter for user names</param>
    ///// <param name="fromDate">Start of period</param>
    ///// <param name="toDate">End of period</param>
    //Task<Result<Table<UserWithActivity>>> GetUsersWithActivityTable(TableParameters requestParameters);

}