namespace Smart.AspNetCore.Filters
{
    using System.Diagnostics;

    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    ///
    /// </summary>
    public class TimeLoggingFilter : IActionFilter
    {
        private const string Key = "_TimeLogging";

        private readonly ILogger<TimeLoggingFilter> logger;

        private readonly TimeLoggingOptions options;

        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public TimeLoggingFilter(ILogger<TimeLoggingFilter> logger, IOptions<TimeLoggingOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items[Key] = Stopwatch.StartNew();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var watch = (Stopwatch)context.HttpContext.Items[Key];
            var elapsed = watch.ElapsedMilliseconds;

            if (watch.ElapsedMilliseconds >= options.Thresold)
            {
                logger.LogWarning("Long execution. Elapsed=[{0}]", elapsed);
            }
        }
    }
}
