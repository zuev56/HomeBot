using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zs.Common.Services.Http;

namespace HomeBot.Features.UserWatcher;

internal sealed class UserWatcher : IUserWatcher
{
    private readonly IConfiguration _configuration;

    public UserWatcher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> DetectLongTimeInactiveUsersAsync(TimeSpan inactiveTime)
    {
        var trackedUserIds = _configuration.GetSection("Home:Vk:TrackedUserIds").Get<int[]>()!;
        var baseUrl = _configuration["Home:Vk:VkActivityApiUrl"];

        var result = new StringBuilder();
        foreach (var userId in trackedUserIds)
        {
            var getActivityUrl = $"{baseUrl}/api/activity/{userId}/last-utc";
            var lastSeen = await Request.GetAsync<DateTime>(getActivityUrl);
            var interval = DateTime.UtcNow - lastSeen;
            if (interval < inactiveTime)
            {
                continue;
            }

            var getUserUrl = $"{baseUrl}/api/users/{userId}";
            var user = await Request.GetAsync<User>(getUserUrl, throwExceptionOnError: true);
            if (user == null)
            {
                continue;
            }

            var userName = $"{user.FirstName} {user.LastName}";
            result.AppendLine($"User {userName} is not active for {interval:hh\\:mm\\:ss}");
        }

        return result.ToString().Trim();
    }
}