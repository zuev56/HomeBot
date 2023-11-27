using System.Threading.Tasks;

namespace HomeBot.Features.Hardware;

public interface IHasCurrentState
{
    Task<string> GetCurrentStateAsync();
}