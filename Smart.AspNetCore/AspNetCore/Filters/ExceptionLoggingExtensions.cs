namespace Smart.AspNetCore.Filters
{
    using System;

    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    ///
    /// </summary>
    public static class ExceptionLoggingExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddExceptionLogging(this IServiceCollection services)
        {
            services.AddOptions();
            services.TryAddSingleton<ExceptionLoggingFilter>();
            services.TryAddSingleton<ExceptionLoggingOptions>();

            return services;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddExceptionLogging(this IServiceCollection services, Action<ExceptionLoggingOptions> setupAction)
        {
            services.AddExceptionLogging();
            services.Configure(setupAction);

            return services;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static IFilterMetadata AddExceptionLogging(this FilterCollection filters)
        {
            return filters.AddService<ExceptionLoggingFilter>();
        }
    }
}
