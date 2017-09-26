namespace Smart.AspNetCore.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    ///
    /// </summary>
    public class ExceptionLoggingFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionLoggingFilter> logger;

        private readonly ExceptionLoggingOptions options;

        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public ExceptionLoggingFilter(ILogger<ExceptionLoggingFilter> logger, IOptions<ExceptionLoggingOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            logger.LogError(options.EventId, context.Exception, options.Message);
        }
    }
}
