using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Hosting.ScheduledJobs.IntegrationTests
{
    [TestFixture]
    public class ExecutionTest
    {
        private IHost _host;

        [SetUp]
        public void SetUp()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddScheduledJob<TestJob>(options =>
                    {
                        options.ExecutionSchedule = "* * * * * *";
                    });
                });

            _host = hostBuilder.Build();
        }

        [Test]
        public async Task ScheduledJobShouldRunAccordingToExecutionSchedule()
        {
            await _host.StartAsync();

            await Task.Delay(2000);

            var wasExecuted = TestJob.WasExecuted;

            await _host.StopAsync();

            Assert.That(wasExecuted, Is.True);
        }

        private class TestJob : IScheduledJob
        {
            public static bool WasExecuted { get; private set; }

            public Task ExecuteAsync()
            {
                WasExecuted = true;

                return Task.CompletedTask;
            }
        }
    }
}
