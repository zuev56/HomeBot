namespace Home.Bot.Abstractions
{
    internal interface IUserWatcher
    {
        Task<string> DetectLongTimeInactiveUsersAsync();
    }
}
