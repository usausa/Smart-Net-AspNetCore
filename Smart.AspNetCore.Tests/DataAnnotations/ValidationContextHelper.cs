namespace Smart.AspNetCore.DataAnnotations;

using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;

internal static class ValidationContextHelper
{
    private static readonly IServiceProvider ServiceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore().AddDataAnnotations();
        return services.BuildServiceProvider();
    }

    public static ValidationContext Create(object instance, string? memberName = null)
    {
        var context = new ValidationContext(instance, ServiceProvider, null);
        if (memberName is not null)
        {
            context.MemberName = memberName;
        }

        return context;
    }
}
