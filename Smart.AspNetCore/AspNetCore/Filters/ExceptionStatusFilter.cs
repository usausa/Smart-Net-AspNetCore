namespace Smart.AspNetCore.Filters
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    using Smart.AspNetCore.Exceptions;

    public sealed class ExceptionStatusFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is HttpStatusException ex)
            {
                context.Result = new StatusCodeResult(ex.StatusCode);
                context.ExceptionHandled = true;
            }
        }
    }
}
