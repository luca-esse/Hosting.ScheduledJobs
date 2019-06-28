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

        private readonly IOptionsMonitor<ScheduledJobOptions<TScheduledJob>> _optionsMonitor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobScheduler<TScheduledJob>> _logger;

        private ScheduledJobOptions<TScheduledJob> _options;
        private IDisposable _optionsChangeListener;

        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly Timer _timer;

        public JobScheduler(
            IOptionsMonitor<ScheduledJobOptions<TScheduledJob>> optionsMonitor,
            IServiceProvider serviceProvider,
            ILogger<JobScheduler<TScheduledJob>> logger
        )
        {
            _optionsMonitor = optionsMonitor;
            _serviceProvider = serviceProvider;
            _logger = logger;

            _options = optionsMonitor.CurrentValue;

            _semaphoreSlim = new SemaphoreSlim(1, 1);
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        private async void TimerCallback(object state)
        {
            try
            {
                StartOrRestartTimer();

                await ExecuteJobAsync(Guid.NewGuid());
            }
            catch (Exception e)
            {
                _logger.LogUnhandledExceptionOnTimer(e);
                throw;
            }
        }

        private async Task ExecuteJobAsync(Guid executionId)
        {
            await _semaphoreSlim.WaitAsync();

            using (_logger.BeginJobExecutionScope(executionId, _scheduledJobTypeName))
            {
                _logger.LogJobExecutionStarted();

                var stopwatch = new Stopwatch();
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    stopwatch.Start();

                    await scope.ServiceProvider
                        .GetRequiredService<TScheduledJob>()
                        .ExecuteAsync();

                    stopwatch.Stop();
                }
                catch (Exception e)
                {
                    stopwatch.Stop();

                    _logger.LogJobExecutionFailed(e, stopwatch.ElapsedMilliseconds);
                }

                _logger.LogJobExecutionCompleted(stopwatch.ElapsedMilliseconds);

                if (stopwatch.ElapsedMilliseconds > _options.SlowWarningThreshold?.TotalMilliseconds)
                    _logger.LogSlowJobExecutionWarning(stopwatch.ElapsedMilliseconds);
            }

            _semaphoreSlim.Release();
        }

        private void StartOrRestartTimer()
        {
            var nextExecutionDueTime = CrontabScheduleUtils.GetNextExecutionDueTime(_options.CrontabExecutionSchedule);

            _timer.StartOrRestart(nextExecutionDueTime);

            _logger.LogTimerStarted((long) nextExecutionDueTime.TotalMilliseconds);
        }

        private void StopTimer()
        {
            _timer.Stop();

            _logger.LogTimerStopped();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartOrRestartTimer();

            _optionsChangeListener = _optionsMonitor.OnChange(options =>
            {
                _options = options;
                StartOrRestartTimer();
            });

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