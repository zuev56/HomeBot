using System.Threading.Tasks;

namespace Home.Bot.Abstractions
{
    internal interface IUserWatcher
    {
        Task<string> DetectLongTimeInactiveUsersAsync();
    }
}