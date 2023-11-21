namespace Smart.AspNetCore;

using System.Net;
using System.Net.Sockets;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Smart.AspNetCore.Filters;

public static class ServiceCollectionExtensions
{
    //--------------------------------------------------------------------------------
    // Exception status
    //--------------------------------------------------------------------------------

    public static IServiceCollection AddExceptionStatus(this IServiceCollection services)
    {
        services.TryAddSingleton<ExceptionStatusFilter>();

        return services;
    }

    public static IFilterMetadata AddExceptionStatus(this FilterCollection filters)
    {
        return filters.AddService<ExceptionStatusFilter>();
    }

    //--------------------------------------------------------------------------------
    // Exception logging
    //--------------------------------------------------------------------------------

    public static IServiceCollection AddExceptionLogging(this IServiceCollection services)
    {
        services.AddOptions();
        services.TryAddSingleton<ExceptionLoggingFilter>();

        return services;
    }

    public static IFilterMetadata AddExceptionLogging(this FilterCollection filters)
    {
        return filters.AddService<ExceptionLoggingFilter>();
    }

    //--------------------------------------------------------------------------------
    // Time logging
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // Path restrict
    //--------------------------------------------------------------------------------

    public static IApplicationBuilder UseWhenFrom(this IApplicationBuilder builder, string path, string[]? networks, Action<IApplicationBuilder> configuration)
    {
        if ((networks is null) || (networks.Length == 0))
        {
            bool Predicate(HttpContext context)
            {
                return context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase);
            }

            builder.UseWhen(Predicate, configuration);
        }
        else
        {
            var ipNetworks = networks
                .Select(static x =>
                {
                    if (IPNetwork.TryParse(x, out var network))
                    {
                        return network;
                    }

                    var address = IPAddress.Parse(x);
                    return new IPNetwork(address, address.AddressFamily == AddressFamily.InterNetwork ? 32 : 128);
                })
                .ToArray();

            bool Predicate(HttpContext context)
            {
                return context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase) &&
                       Array.Exists(ipNetworks, network => (context.Request.HttpContext.Connection.RemoteIpAddress is not null) &&
                                                           network.Contains(context.Request.HttpContext.Connection.RemoteIpAddress));
            }

            builder.UseWhen(Predicate, configuration);
        }

        return builder;
    }
}
