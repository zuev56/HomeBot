using Zs.Common.Services.Scheduling;

namespace HomeBot.Features.Hardware;

public interface IHasJob
{
    public ProgramJob<string> Job { get; }
}