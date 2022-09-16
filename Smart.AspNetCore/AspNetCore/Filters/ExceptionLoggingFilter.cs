namespace Smart.AspNetCore.Filters;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class ExceptionLoggingFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionLoggingFilter> logger;

    private readonly ExceptionLoggingOptions options;

    public ExceptionLoggingFilter(ILogger<ExceptionLoggingFilter> logger, IOptions<ExceptionLoggingOptions> options)
    {
        this.logger = logger;
        this.options = options.Value;
    }

    public void OnException(ExceptionContext context)
    {
#pragma warning disable CA2254
        logger.LogError(options.EventId, context.Exception, options.Message);
#pragma warning restore CA2254
    }
}
