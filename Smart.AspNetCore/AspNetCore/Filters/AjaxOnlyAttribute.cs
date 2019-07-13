namespace Smart.AspNetCore.Filters
{
    using System;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    using Smart.AspNetCore.Http;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AjaxOnlyAttribute : ActionFilterAttribute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Ignore")]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.IsAjaxRequest())
            {
                context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
        }
    }
}
