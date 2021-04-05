namespace Smart.AspNetCore.Filters
{
    using System;

    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class ExceptionLoggingExtensions
    {
        public static IServiceCollection AddExceptionLogging(this IServiceCollection services)
        {
            services.AddOptions();
            services.TryAddSingleton<ExceptionLoggingFilter>();
            services.TryAddSingleton<ExceptionLoggingOptions>();

            return services;
        }

        public static IServiceCollection AddExceptionLogging(this IServiceCollection services, Action<ExceptionLoggingOptions> setupAction)
        {
            services.AddExceptionLogging();
            services.Configure(setupAction);

            return services;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static IFilterMetadata AddExceptionLogging(this FilterCollection filters)
        {
            return filters.AddService<ExceptionLoggingFilter>();
        }
    }
}
