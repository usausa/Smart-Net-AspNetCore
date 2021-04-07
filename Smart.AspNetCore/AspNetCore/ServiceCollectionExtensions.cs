namespace Smart.AspNetCore
{
    using System;

    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using Smart.AspNetCore.Filters;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTimeLogging(this IServiceCollection services)
        {
            services.AddOptions();
            services.TryAddSingleton<TimeLoggingFilter>();
            services.TryAddSingleton<TimeLoggingOptions>();
            return services;
        }

        public static IServiceCollection AddTimeLogging(this IServiceCollection services, Action<TimeLoggingOptions> setupAction)
        {
            services.AddTimeLogging();
            services.Configure(setupAction);
            return services;
        }

        public static IFilterMetadata AddTimeLogging(this FilterCollection filters)
        {
            return filters.AddService<TimeLoggingFilter>();
        }
    }
}
