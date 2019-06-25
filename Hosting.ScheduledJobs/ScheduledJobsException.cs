using System;

namespace Hosting.ScheduledJobs
{
    /// <summary>
    ///     An exception inherited by Hosting.ScheduledJobs exceptions.
    /// </summary>
    public abstract class ScheduledJobsException : Exception
    {
        internal ScheduledJobsException(string message) : base(message)
        {
            
        }
    }
}