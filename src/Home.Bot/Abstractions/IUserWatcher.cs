using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Services.Abstractions;

namespace Home.Bot.Abstractions
{
    internal interface IUserWatcher
    {
        public IReadOnlyCollection<IJobBase> Jobs { get; }

        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
