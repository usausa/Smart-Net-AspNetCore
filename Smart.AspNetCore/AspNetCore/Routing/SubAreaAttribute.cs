namespace Smart.AspNetCore.Routing;

using Microsoft.AspNetCore.Mvc.Routing;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SubAreaAttribute : RouteValueAttribute
{
    public SubAreaAttribute(string routeValue)
        : base("subarea", routeValue)
    {
    }
}
