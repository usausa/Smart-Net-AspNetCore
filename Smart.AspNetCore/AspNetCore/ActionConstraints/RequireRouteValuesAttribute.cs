namespace Smart.AspNetCore.ActionConstraints
{
    using System;

    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Routing;

    /// <summary>
    ///
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRouteValuesAttribute : ActionMethodSelectorAttribute
    {
        private readonly string[] values;

        /// <summary>
        ///
        /// </summary>
        /// <param name="values"></param>
        public RequireRouteValuesAttribute(params string[] values)
        {
            this.values = values;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="routeContext"></param>
        /// <param name="action"></param>
        /// <returns></returns>
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
