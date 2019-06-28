using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hosting.ScheduledJobs
{
    /// <summary>
    ///     Extension methods for adding scheduled jobs to the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds a scheduled job to the DI container with the provided configuration.
        /// </summary>
        /// <param name="serviceCollection">
        ///     The <see cref="IServiceCollection" /> to add the services to.
        /// </param>
        /// <param name="configuration">
        ///     The configuration of the job scheduling.
        /// </param>
        /// <typeparam name="TScheduledJob">
        ///     The type of scheduled job being configured.
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IServiceCollection" /> so that additional calls can be chained.
        /// </returns>
        public static IServiceCollection AddScheduledJob<TScheduledJob>
        (
            this IServiceCollection serviceCollection,
            IConfiguration configuration
        )
            where TScheduledJob : class, IScheduledJob
        {
            serviceCollection
                .AddOptions<ScheduledJobOptions<TScheduledJob>>()
                .Bind(configuration)
                .ValidateDataAnnotations();

            return AddServices<TScheduledJob>(serviceCollection);
        }

        /// <summary>
        ///     Adds a scheduled job to the DI container with the provided configuration.
        /// </summary>
        /// <param name="serviceCollection">
        ///     The <see cref="IServiceCollection" /> to add the services to.
        /// </param>
        /// <param name="configureOptions">
        ///     The action used to configure the job scheduling.
        /// </param>
        /// <typeparam name="TScheduledJob">
        ///     The type of scheduled job being configured.
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IServiceCollection" /> so that additional calls can be chained.
        /// </returns>
        public static IServiceCollection AddScheduledJob<TScheduledJob>
        (
            this IServiceCollection serviceCollection,
            Action<ScheduledJobOptions<TScheduledJob>> configureOptions
        )
            where TScheduledJob : class, IScheduledJob
        {
            serviceCollection
                .AddOptions<ScheduledJobOptions<TScheduledJob>>()
                .Configure(configureOptions)
                .ValidateDataAnnotations();

            return AddServices<TScheduledJob>(serviceCollection);
        }

        private static IServiceCollection AddServices<T>(IServiceCollection serviceCollection) 
            where T : class, IScheduledJob
        {
            serviceCollection.AddHostedService<JobScheduler<T>>();
            serviceCollection.AddScoped<T>();
            return serviceCollection;
        }
    }
}
