namespace Smart.AspNetCore.ActionConstraints
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Routing;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class QueryParameterAttribute : ActionMethodSelectorAttribute
    {
        private readonly string name;

        private readonly string[] values;

        public QueryParameterAttribute(string name, params object[] values)
        {
            this.name = name;
            this.values = values.Select(x => x.ToString()!).ToArray();
        }

        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            return routeContext.HttpContext.Request.Query.TryGetValue(name, out var queryValues) &&
                   ((values.Length == 0) ||
                    queryValues.Any(x => values.Any(y => String.Equals(x, y, StringComparison.OrdinalIgnoreCase))));
        }
    }
}
