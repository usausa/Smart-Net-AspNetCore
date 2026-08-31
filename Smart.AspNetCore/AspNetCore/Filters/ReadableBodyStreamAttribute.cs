namespace Smart.AspNetCore.Filters;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

#pragma warning disable CA1034
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ReadableBodyStreamAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => true;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new ReadableBodyStreamFilter();
    }

    public sealed class ReadableBodyStreamFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            context.HttpContext.Request.EnableBuffering();
        }
    }
}
#pragma warning restore CA1034
