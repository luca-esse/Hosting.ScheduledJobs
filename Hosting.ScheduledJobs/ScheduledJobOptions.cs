using System;
using NCrontab;

namespace Hosting.ScheduledJobs
{
    /// <summary>
    ///     Options for a scheduled job.
    /// </summary>
    /// <typeparam name="TScheduledJob">
    ///     The type of scheduled job that owns the options.
    /// </typeparam>
    public class ScheduledJobOptions<TScheduledJob> where TScheduledJob : IScheduledJob
    {
        private readonly CrontabSchedule.ParseOptions _parseOptions = new CrontabSchedule.ParseOptions
        {
            IncludingSeconds = true
        };

        private string _executionSchedule;
        private TimeSpan? _slowWarningThreshold;

        internal CrontabSchedule CrontabExecutionSchedule { get; private set; }

        /// <summary>
        ///     Defines the job schedule with a crontab expression. 
        /// </summary>
        /// <exception cref="ArgumentNullException">The crontab expression is null.</exception>
        /// <exception cref="ArgumentException">The crontab expression is not valid.</exception>
        public string ExecutionSchedule
        {
            get => _executionSchedule;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                try
                {
                    CrontabExecutionSchedule = CrontabSchedule.Parse(value, _parseOptions);
                    _executionSchedule = value;
                }
                catch (CrontabException e)
                {
                    throw new ArgumentException("Invalid crontab expression.", nameof(value), e);
                }
            }
        }

        /// <summary>
        ///     Defines a threshold for logging a warning when the job takes too long
        ///     to complete the execution.
        /// </summary>
        /// <remarks>
        ///     When null the warning is disabled.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The warning threshold is less or equal to 0.</exception>
        public TimeSpan? SlowWarningThreshold
        {
            get => _slowWarningThreshold;
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(value), "The warning threshold is less or equal to 0.");

                _slowWarningThreshold = value;
            }
        }
    }
}