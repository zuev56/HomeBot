using System;
using System.Text;
using System.Threading.Tasks;
using Home.Bot.Abstractions;
using Home.Bot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Abstractions;
using Zs.Common.Services.WebAPI;

namespace Home.Bot.Services
{
    internal class UserWatcher : IUserWatcher
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserWatcher> _logger;

        public UserWatcher(
            IUsersRepository usersRepository,
            IConfiguration configuration,
            ILogger<UserWatcher> logger)
        {
            _usersRepository = usersRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> DetectLongTimeInactiveUsersAsync()
        {
            var trackedUserIds = _configuration.GetSection("Home:Vk:TrackedUserIds").Get<int[]>();
            var inactiveHoursLimit = _configuration.GetValue<int>("Home:Vk:InactiveHoursLimit");
            var baseUrl = _configuration["Home:Vk:VkActivityApiUrl"];

            var result = new StringBuilder();
            foreach (var userId in trackedUserIds)
            {
                var getActivityUrl = $"{baseUrl}/api/activity/{userId}/last-utc";
                var lastSeen = await ApiHelper.GetAsync<DateTime>(getActivityUrl);
                var interval = DateTime.UtcNow - lastSeen;
                if (interval >= TimeSpan.FromHours(inactiveHoursLimit))
                {
                    var getUserUrl = $"{baseUrl}/api/users/{userId}";
                    var user = await ApiHelper.GetAsync<UserDto>(getUserUrl, throwExceptionOnError: true);
                    if (user != null)
                    {
                        var userName = $"{user.FirstName} {user.LastName}";
                        result.AppendLine($"User {userName} is not active for {interval:hh\\:mm\\:ss}");
                    }
                }
            }
            
            return result.ToString().Trim();
        }
    }
}