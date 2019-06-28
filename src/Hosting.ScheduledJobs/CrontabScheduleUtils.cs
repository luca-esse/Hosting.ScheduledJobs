using System;
using NCrontab;

namespace Hosting.ScheduledJobs
{
    internal static class CrontabScheduleUtils
    {
        internal static TimeSpan GetNextExecutionDueTime(CrontabSchedule crontabSchedule)
        {
            var now = DateTime.UtcNow;
            var nextExecution = crontabSchedule.GetNextOccurrence(now);
            var intervalToNextExecution = nextExecution - now;
            return intervalToNextExecution;
        }
    }
}