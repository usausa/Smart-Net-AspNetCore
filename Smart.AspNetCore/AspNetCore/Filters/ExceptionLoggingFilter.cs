namespace Smart.AspNetCore.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///
    /// </summary>
    public class ExceptionLoggingFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionLoggingFilter> logger;

        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        public ExceptionLoggingFilter(ILogger<ExceptionLoggingFilter> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            logger.LogError(0, context.Exception, "Handle exception.");
        }
    }
}
