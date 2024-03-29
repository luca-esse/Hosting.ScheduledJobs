# Scheduled jobs for Generic Host and ASP.NET Core Web Host

```csharp
public static async Task Main(string[] args)
{
    var host = new HostBuilder()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddScheduledJob<MyJob>(options =>
            {
                // Crontab expression: At 04:05 on Sunday
                options.ExecutionSchedule = "5 4 * * sun";
            });
        })
        .Build();

    await host.RunAsync();
}

public class MyJob : IScheduledJob
{
    public Task ExecuteAsync()
    {
        // Do work.
        return Task.CompletedTask;
    }
}
```
