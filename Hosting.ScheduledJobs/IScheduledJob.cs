using System.Threading.Tasks;

namespace Hosting.ScheduledJobs
{
    /// <summary>
    ///     Represents a scheduled job. 
    /// </summary>
    public interface IScheduledJob
    {
        /// <summary>
        ///     Triggered when the configured execution interval is elapsed.
        /// </summary>
        /// <remarks>
        ///     Job executions are not concurrent.
        ///     The next execution is queued and it won't start until the current execution is completed. 
        /// </remarks>
        Task ExecuteAsync();
    }
}