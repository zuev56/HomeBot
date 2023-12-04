using System;
using System.Text;
using System.Threading.Tasks;
using HomeBot.Features.Hardware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Common.Services.Http;
using Zs.Common.Services.Scheduling;
using static HomeBot.Features.VkUsers.Constants;

namespace HomeBot.Features.VkUsers;

internal sealed class UserWatcher : IHasJob, IHasCurrentState
{
    private readonly UserWatcherSettings _options;
    public ProgramJob<string> Job { get; }

    public UserWatcher(IOptions<UserWatcherSettings> options, ILogger<UserWatcher> logger)
    {
        _options = options.Value;

        Job = new ProgramJob<string>(
            period: 5.Minutes(),
            method: DetectInactiveUsersAsync,
            description: InactiveUsersInformer,
            startUtcDate: DateTime.UtcNow + 5.Seconds(),
            logger: logger);
    }

    private async Task<string> DetectInactiveUsersAsync()
    {
        var result = new StringBuilder();
        foreach (var userId in _options.TrackedIds)
        {
            var inactiveTime = await GetInactiveTimeAsync(userId);
            if (inactiveTime < TimeSpan.FromHours(_options.InactiveHoursLimit)) //TODO: добавить в Zs.Common double.Hours())
                continue;

            var user = await GetUserAsync(userId);
            if (user == null)
                continue;

            var userName = $"{user.FirstName} {user.LastName}";
            result.AppendLine($@"User {userName} is not active for {inactiveTime:hh\:mm\:ss}");
        }

        return result.ToString().Trim();
    }

    private async Task<User?> GetUserAsync(int userId)
    {
        var url = $"{_options.VkActivityApiUri}/api/users/{userId}";
        var user = await Request.Create(url).GetAsync<User>();
        return user;
    }

    private async Task<TimeSpan> GetInactiveTimeAsync(int userId)
    {
        var url = $"{_options.VkActivityApiUri}/api/activity/{userId}/last-utc";
        var lastSeen = await Request.Create(url).GetAsync<DateTime>();
        var inactiveTime = DateTime.UtcNow - lastSeen;
        return inactiveTime;
    }

    public async Task<string> GetCurrentStateAsync()
    {
        var result = new StringBuilder();
        foreach (var userId in _options.TrackedIds)
        {
            var inactiveTime = await GetInactiveTimeAsync(userId);
            var user = await GetUserAsync(userId);
            var userName = $"{user!.FirstName} {user.LastName}";
            result.AppendLine($@"User {userName} is not active for {inactiveTime:hh\:mm\:ss}");
        }

        return result.ToString().Trim();
    }
}