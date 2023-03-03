using Home.Data.Models.Vk;
using Home.Services.Vk.Models;
using Home.Web.Areas.ApiVk.Models;
using Home.Web.Areas.App.Models.Vk;
using System.Collections.Generic;
using System.Linq;
using Zs.Common.Models;

namespace Home.Web.Areas.App.Services
{
    public class VkMapper
    {
        internal IEnumerable<UserVM> ToUsersVM(Result<List<User>> users)
        {
            // TODO: handle error
            return users.Value.Select(u => ToUserVM(u));
        }

        internal IEnumerable<UserVM> ToUsersVMWithActivity(Result<List<UserWithActivity>> usersWithActivity)
        {
            // TODO: handle error
            return usersWithActivity.Value.Select(u => ToUserVM(u.User, u.ActivitySec));
        }

        internal DetailedUserActivityVM ToDetailedUserActivityVM(Result<DetailedUserActivity> activity)
        {
            // TODO: handle error
            return new DetailedUserActivityVM
            {
                UserName             = activity.Value.UserName,
                Url                  = activity.Value.Url,
                AnalyzedDaysCount    = activity.Value.AnalyzedDaysCount,
                ActivityDaysCount    = activity.Value.ActivityDaysCount,
                BrowserEntrance      = activity.Value.VisitsFromSite,
                MobileEntrance       = activity.Value.VisitsFromApp,
                BrowserActivityTime  = activity.Value.TimeInSite,
                MobileActivityTime   = activity.Value.TimeInApp,
                ActivityCalendar     = activity.Value.ActivityCalendar,
                AvgDailyActivityTime = activity.Value.AvgDailyTime,
                MinDailyActivityTime = activity.Value.MinDailyTime,
                MaxDailyActivityTime = activity.Value.MaxDailyTime,
                AvgWeekDayActivity   = activity.Value.AvgWeekDayActivity
            };
        }

        internal PeriodUserActivityVM ToPeriodUserActivityVM(Result<SimpleUserActivity> activity)
        {
            // TODO: handle error
            return new PeriodUserActivityVM
            {
                UserId              = activity.Value.UserId,
                UserName            = activity.Value.UserName,
                CurrentStatus       = activity.Value.CurrentStatus,
                Url                 = $"https://vk.com/id{activity.Value.UserId}",
                //FromDate            = activity.Value.FromDate,
                //ToDate              = activity.Value.ToDate,
                BrowserActivityTime = activity.Value.TimeInSite,
                MobileActivityTime  = activity.Value.TimeInApp,
                EntranceCounter     = activity.Value.VisitsCount
            };
        }

        private static UserVM ToUserVM(User u, int activitySec = -1)
            => new UserVM
            {
                Id = u.Id,
                UserName = $"{u.FirstName} {u.LastName}",
                ActivitySec = activitySec
            };

    }
}