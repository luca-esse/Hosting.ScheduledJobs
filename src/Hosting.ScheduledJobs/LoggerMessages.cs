using System;
using Microsoft.Extensions.Logging;

namespace Hosting.ScheduledJobs
{
    internal static class LoggerMessages
    {
        private static readonly Action<ILogger, Exception> _logUnhandledExceptionOnTimer = LoggerMessage.Define(
            LogLevel.Critical,
            EventIds.UnhandledExceptionOnTimer,
            "Unhandled exception on timer callback"
        );

        internal static void LogUnhandledExceptionOnTimer(this ILogger logger, Exception exception)
            => _logUnhandledExceptionOnTimer(logger, exception);

        internal static IDisposable BeginJobExecutionScope(this ILogger logger, Guid executionId, string scheduledJobTypeName)
            => _beginJobExecutionScope(logger, executionId, scheduledJobTypeName);

        private static readonly Func<ILogger, Guid, string, IDisposable> _beginJobExecutionScope = LoggerMessage.DefineScope<Guid, string>(
            "Scheduled job execution (Execution Id: {executionId}, Job type name: {scheduledJobTypeName})"
        );

        private static readonly Action<ILogger, long, Exception> _logJobExecutionFailed = LoggerMessage.Define<long>(
            LogLevel.Error,
            EventIds.JobExecutionFailed,
            "Execution failed after {elapsedMilliseconds}ms"
        );
        
        internal static void LogJobExecutionFailed(this ILogger logger, Exception exception, long elapsedMilliseconds)
            => _logJobExecutionFailed(logger, elapsedMilliseconds, exception);

        private static readonly Action<ILogger, Exception> _logJobExecutionStarted = LoggerMessage.Define(
            LogLevel.Information,
            EventIds.JobExecutionStarted,
            "Execution started"
        );

        internal static void LogJobExecutionStarted(this ILogger logger)
            => _logJobExecutionStarted(logger, null);

        private static readonly Action<ILogger, long, Exception> _logJobExecutionCompleted = LoggerMessage.Define<long>(
            LogLevel.Information,
            EventIds.JobExecutionCompleted,
            "Execution completed after {elapsedMilliseconds}ms"
        );

        internal static void LogJobExecutionCompleted(this ILogger logger, long elapsedMilliseconds)
            => _logJobExecutionCompleted(logger, elapsedMilliseconds, null);

        private static readonly Action<ILogger, long, Exception> _logSlowJobExecutionWarning = LoggerMessage.Define<long>(
            LogLevel.Warning,
            EventIds.SlowJobExecutionWarning,
            "Execution took too long: {elapsedMilliseconds}ms"
        );

        internal static void LogSlowJobExecutionWarning(this ILogger logger, long elapsedMilliseconds)
            => _logSlowJobExecutionWarning(logger, elapsedMilliseconds, null);

        internal static void LogTimerStarted(this ILogger logger, long durationMilliseconds)
            => _logTimerStarted(logger, durationMilliseconds, null);

        private static readonly Action<ILogger, long, Exception> _logTimerStarted = LoggerMessage.Define<long>(
            LogLevel.Debug,
            EventIds.TimerStarted,
            "Timer started with duration: {durationMilliseconds}ms"
        );

        internal static void LogTimerStopped(this ILogger logger)
            => _logTimerStopped(logger, null);

        private static readonly Action<ILogger, Exception> _logTimerStopped = LoggerMessage.Define(
            LogLevel.Debug,
            EventIds.TimerStopped,
            "Timer stopped"
        );

        internal static class EventIds
        {
            
            public static EventId UnhandledExceptionOnTimer { get; } = new EventId(0, nameof(UnhandledExceptionOnTimer));
            public static EventId JobExecutionStarted { get; } = new EventId(1, nameof(JobExecutionStarted));
            public static EventId JobExecutionCompleted { get; } = new EventId(2, nameof(JobExecutionCompleted));
            public static EventId JobExecutionFailed { get; } = new EventId(3, nameof(JobExecutionFailed));
            public static EventId SlowJobExecutionWarning { get; } = new EventId(4, nameof(SlowJobExecutionWarning));
            public static EventId TimerStarted { get; } = new EventId(5, nameof(TimerStarted));
            public static EventId TimerStopped { get; } = new EventId(6, nameof(TimerStopped));
        }
    }
}
