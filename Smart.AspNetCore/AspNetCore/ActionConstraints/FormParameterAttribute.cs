namespace Smart.AspNetCore.ActionConstraints
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Routing;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class FormParameterAttribute : ActionMethodSelectorAttribute
    {
        private readonly string name;

        private readonly string[] values;

        public FormParameterAttribute(string name, params string[] values)
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

            return routeContext.HttpContext.Request.Form.TryGetValue(name, out var formValues) &&
                   ((values.Length == 0) ||
                    formValues.Any(x => values.Any(y => String.Equals(x, y, StringComparison.OrdinalIgnoreCase))));
        }
    }
}
