using System;
using System.Threading;

namespace Hosting.ScheduledJobs
{
    internal static class TimerExtensions
    {
        internal static void Stop(this Timer timer)
        {
            timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        internal static void StartOrRestart(this Timer timer, TimeSpan dueTime)
        {
            timer.Change(dueTime, Timeout.InfiniteTimeSpan);
        }
    }
}
