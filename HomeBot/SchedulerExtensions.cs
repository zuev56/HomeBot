using System;
using System.Linq;
using Zs.Common.Models;
using Zs.Common.Services.Scheduling;

namespace HomeBot;

public static class SchedulerExtensions
{
    public static void SetDefaultExecutionCompletedHandler<TResult>(this IScheduler scheduler,
        Action<Job<TResult>, Result<TResult>> executionCompleted)
    {
        var jobs = scheduler.Jobs.Where(job => job is Job<TResult>).Cast<Job<TResult>>();
        foreach (var job in jobs)
        {
            job.ExecutionCompleted -= executionCompleted;
            job.ExecutionCompleted += executionCompleted;
        }
    }
}