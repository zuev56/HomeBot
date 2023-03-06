using System.Threading.Tasks;

namespace HomeBot.Abstractions;

internal interface IUserWatcher
{
    Task<string> DetectLongTimeInactiveUsersAsync();
}