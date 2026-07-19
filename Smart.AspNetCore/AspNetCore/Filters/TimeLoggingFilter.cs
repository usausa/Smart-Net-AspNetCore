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
        if (!context.HttpContext.Items.TryGetValue(options.Key, out var item) || (item is not Stopwatch watch))
        {
            return;
        }

        var elapsed = watch.ElapsedMilliseconds;
        var isLongExecution = elapsed >= options.Threshold;
        if (isLongExecution)
        {
            logger.WarnLongExecution(elapsed);
        }

        if ((options.HeaderType == TimeLoggingHeaderType.Always) ||
            (options.HeaderType == TimeLoggingHeaderType.LongExecution && isLongExecution))
        {
            context.HttpContext.Response.Headers[options.Header] = $"{elapsed}";
        }
    }
}
