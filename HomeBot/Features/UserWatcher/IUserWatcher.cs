using System;
using System.Threading.Tasks;

namespace HomeBot.Features.UserWatcher;

internal interface IUserWatcher
{
    Task<string> DetectLongTimeInactiveUsersAsync(TimeSpan inactiveTime);
}