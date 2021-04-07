namespace Smart.AspNetCore.Filters
{
    using System;

    using Microsoft.AspNetCore.Mvc.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AjaxModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Response.Headers["X-AjaxRequest-ModelState"] = context.ModelState.IsValid ? "true" : "false";
        }
    }
}
