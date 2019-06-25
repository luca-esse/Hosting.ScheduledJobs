using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hosting.ScheduledJobs
{
    internal class JobScheduler<TScheduledJob> : IHostedService, IDisposable where TScheduledJob : IScheduledJob
    {
        private readonly string _scheduledJobTypeName = typeof(TScheduledJob).Name;

        private readonly ILogger<JobScheduler<TScheduledJob>> _logger;
        private readonly IServiceProvider _serviceProvider;
        private ScheduledJobOptions<TScheduledJob> _options;

        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly Timer _timer;

        private readonly IDisposable _optionsChangeListener;

        public JobScheduler(
            IOptionsMonitor<ScheduledJobOptions<TScheduledJob>> config,
            IServiceProvider serviceProvider,
            ILogger<JobScheduler<TScheduledJob>> logger
        )
        {
            _options = config.CurrentValue;
            _serviceProvider = serviceProvider;
            _logger = logger;

            _semaphoreSlim = new SemaphoreSlim(1, 1);
            _timer = new Timer(async state =>
            {
                await Execute();
            }, null, Timeout.Infinite, Timeout.Infinite);

            _optionsChangeListener = config.OnChange(options =>
            {
                _options = options;
                StartTimer();
            });
        }

        private async Task Execute()
        {
            StartTimer();

            await _semaphoreSlim.WaitAsync();

            using (_logger.BeginScope("Scheduled job of type: {scheduledJobTypeName} execution", _scheduledJobTypeName))
            {
                _logger.LogInformation("Starting execution");

                var stopwatch = new Stopwatch();
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        stopwatch.Start();

                        await scope.ServiceProvider
                            .GetRequiredService<TScheduledJob>()
                            .ExecuteAsync()
                            .ConfigureAwait(false);

                        stopwatch.Stop();
                    }
                }
                catch (Exception e)
                {
                    stopwatch.Stop();

                    _logger.LogError(
                        e,
                        "Execution failed after {durationMilliseconds}ms",
                        stopwatch.ElapsedMilliseconds
                    );
                }
                finally
                {
                    _semaphoreSlim.Release();

                    _logger.LogInformation(
                        "Execution completed after {durationMilliseconds}ms",
                        stopwatch.ElapsedMilliseconds
                    );

                    if (stopwatch.ElapsedMilliseconds > _options.SlowWarningThreshold?.TotalMilliseconds)
                        _logger.LogWarning(
                            "Execution took too long: {durationMilliseconds}ms",
                            stopwatch.ElapsedMilliseconds
                        );
                }
            }
        }

        private void StartTimer()
        {
            if (_options.CrontabExecutionSchedule == null)
                throw new ExecutionScheduleIsNullException();

            var now = DateTime.UtcNow;
            var nextExecution = _options.CrontabExecutionSchedule.GetNextOccurrence(now);
            var intervalToNextExecution = nextExecution - now;
            _timer.Change(intervalToNextExecution, Timeout.InfiniteTimeSpan);
        }

        private void StopTimer()
        {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartTimer();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            StopTimer();
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            _timer?.Dispose();
            _semaphoreSlim?.Dispose();
            _optionsChangeListener?.Dispose();
        }
    }
}