namespace Smart.AspNetCore.Filters;

using System.Diagnostics;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class TimeLoggingFilter : IActionFilter
{
    private readonly ILogger<TimeLoggingFilter> logger;

    private readonly TimeLoggingOptions options;

    public TimeLoggingFilter(ILogger<TimeLoggingFilter> logger, IOptions<TimeLoggingOptions> options)
    {
        this.logger = logger;
        this.options = options.Value;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Items[options.Key] = Stopwatch.StartNew();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var watch = (Stopwatch)context.HttpContext.Items[options.Key]!;
        var elapsed = watch.ElapsedMilliseconds;

        if (watch.ElapsedMilliseconds >= options.Threshold)
        {
            logger.WarnLongExecution(elapsed);

            if (options.HeaderType == TimeLoggingHeaderType.LongExecution)
            {
                context.HttpContext.Response.Headers[options.Header] = $"{elapsed}";
            }
        }
        else if (options.HeaderType == TimeLoggingHeaderType.Always)
        {
            context.HttpContext.Response.Headers[options.Header] = $"{elapsed}";
        }
    }
}
