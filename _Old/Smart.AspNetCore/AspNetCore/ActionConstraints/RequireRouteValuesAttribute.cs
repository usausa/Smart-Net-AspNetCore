namespace Smart.AspNetCore.ActionConstraints
{
    using System;

    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Routing;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireRouteValuesAttribute : ActionMethodSelectorAttribute
    {
        private readonly string[] values;

        public RequireRouteValuesAttribute(params string[] values)
        {
            this.values = values;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Ignore")]
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            foreach (var value in values)
            {
                if (routeContext.RouteData.Values.ContainsKey(value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
