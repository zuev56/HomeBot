using Home.Data.Models.Vk;

namespace Home.Services.Vk.Models;

public class UserWithActivity
{
    public User User { get; init; }
    public int ActivitySec { get; init; }
    public bool isOnline { get; init; }
}
