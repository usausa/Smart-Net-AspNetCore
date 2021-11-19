namespace Smart.AspNetCore.ActionConstraints;

using System;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

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

        return routeContext.HttpContext.Request.Form.TryGetValue(name, out var formValues) && Match(values, formValues);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Match(string[] values, StringValues formValues)
    {
        if (values.Length == 0)
        {
            return true;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (formValues.Contains(values[i]))
            {
                return true;
            }
        }

        return false;
    }
}
