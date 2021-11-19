namespace Smart.AspNetCore.ActionConstraints;

using System;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

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
        return routeContext.HttpContext.Request.Query.TryGetValue(name, out var queryValues) && Match(values, queryValues);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Match(string[] values, StringValues queryValues)
    {
        if (values.Length == 0)
        {
            return true;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (queryValues.Contains(values[i]))
            {
                return true;
            }
        }

        return false;
    }
}
