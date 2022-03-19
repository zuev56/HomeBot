using Home.Data.Models.Vk;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.Bot.Data.Abstractions;

namespace Home.Data.Abstractions
{
    public interface IVkUsersRepository : IRepository<Data.Models.Vk.User, int>
    {
        Task<List<User>> FindAllWhereNameLikeValueAsync(string value, int? skip, int? take);
    }
}
