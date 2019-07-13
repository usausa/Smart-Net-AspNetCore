namespace Smart.AspNetCore.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public sealed class ExceptionLoggingFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionLoggingFilter> logger;

        private readonly ExceptionLoggingOptions options;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Ignore")]
        public ExceptionLoggingFilter(ILogger<ExceptionLoggingFilter> logger, IOptions<ExceptionLoggingOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Ignore")]
        public void OnException(ExceptionContext context)
        {
            logger.LogError(options.EventId, context.Exception, options.Message);
        }
    }
}
