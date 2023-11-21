namespace Smart.AspNetCore.Filters;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

public sealed class ExceptionLoggingFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionLoggingFilter> logger;

    public ExceptionLoggingFilter(ILogger<ExceptionLoggingFilter> logger)
    {
        this.logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        logger.ErrorUnhandledException(context.Exception);
    }
}
