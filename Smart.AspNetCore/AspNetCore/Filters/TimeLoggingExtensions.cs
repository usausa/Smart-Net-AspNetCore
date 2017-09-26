namespace Smart.AspNetCore.Filters
{
    using System;

    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    ///
    /// </summary>
    public static class TimeLoggingExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTimeLogging(this IServiceCollection services)
        {
            services.AddOptions();
            services.TryAddSingleton<TimeLoggingFilter>();
            services.TryAddSingleton<TimeLoggingOptions>();

            return services;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddTimeLogging(this IServiceCollection services, Action<TimeLoggingOptions> setupAction)
        {
            services.AddTimeLogging();
            services.Configure(setupAction);

            return services;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static IFilterMetadata AddTimeLogging(this FilterCollection filters)
        {
            return filters.AddService<TimeLoggingFilter>();
        }
    }
}
