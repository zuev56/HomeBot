using System.Collections.Generic;
using System.Threading.Tasks;
using Home.Data.Models.Vk;
using Zs.Common.Abstractions;

namespace Home.Services.Vk;

public interface IActivityLoggerService
{
    /// <summary> Add new Vk user ID</summary>
    /// <param name="userIds">VK user ID</param>
    Task<IOperationResult<List<User>>> AddNewUsersAsync(params int[] userIds);

    /// <summary> Activity data collection </summary>
    Task<IOperationResult> SaveVkUsersActivityAsync();
}
