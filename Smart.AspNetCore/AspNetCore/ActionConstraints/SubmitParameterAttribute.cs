namespace Smart.AspNetCore.ActionConstraints
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Routing;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SubmitParameterAttribute : ActionMethodSelectorAttribute
    {
        private readonly string name;

        private readonly string[] values;

        public SubmitParameterAttribute(string name, params string[] values)
        {
            this.name = name;
            this.values = values;
        }

        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            if (!routeContext.HttpContext.Request.HasFormContentType)
            {
                return false;
            }

            var formValues = routeContext.HttpContext.Request.Form[name];
            return (formValues.Count > 0) &&
                   ((values.Length == 0) ||
                    formValues.Any(x => values.Any(y => String.Equals(x, y, StringComparison.OrdinalIgnoreCase))));
        }
    }
}
