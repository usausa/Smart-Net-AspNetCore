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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254", Justification = "Ignore")]
    public void OnActionExecuted(ActionExecutedContext context)
    {
        var watch = (Stopwatch)context.HttpContext.Items[options.Key]!;
        var elapsed = watch.ElapsedMilliseconds;

        if (watch.ElapsedMilliseconds >= options.Threshold)
        {
            logger.LogWarning(options.Message, elapsed);
        }
    }
}
