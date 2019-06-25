namespace Hosting.ScheduledJobs
{
    /// <summary>
    ///     An exception throw when <see cref="ScheduledJobOptions{TScheduledJob}.CrontabExecutionSchedule"/> is null. 
    /// </summary>
    public class ExecutionScheduleIsNullException : ScheduledJobsException
    {
        internal ExecutionScheduleIsNullException() : base("The crontab execution schedule supplied in the options is null.")
        {
        }
    }
}