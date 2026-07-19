namespace Smart.AspNetCore.ActionConstraints;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

public sealed class FormParameterAttributeTest
{
    [Fact]
    public void MatchesWhenFormValueIsInAllowedSet()
    {
        var attribute = new FormParameterAttribute("mode", "a", "b");

        Assert.True(attribute.IsValidForRequest(CreateFormRouteContext("mode", "b"), new ActionDescriptor()));
    }

    [Fact]
    public void DoesNotMatchWhenFormValueIsNotInAllowedSet()
    {
        var attribute = new FormParameterAttribute("mode", "a", "b");

        Assert.False(attribute.IsValidForRequest(CreateFormRouteContext("mode", "c"), new ActionDescriptor()));
    }

    [Fact]
    public void DoesNotMatchWhenNotFormContentType()
    {
        var attribute = new FormParameterAttribute("mode", "a");
        var routeContext = new RouteContext(new DefaultHttpContext());

        Assert.False(attribute.IsValidForRequest(routeContext, new ActionDescriptor()));
    }

    [Fact]
    public void DoesNotMatchWhenFormKeyIsMissing()
    {
        var attribute = new FormParameterAttribute("mode", "a");

        Assert.False(attribute.IsValidForRequest(CreateFormRouteContext("other", "a"), new ActionDescriptor()));
    }

    private static RouteContext CreateFormRouteContext(string key, string value)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.ContentType = "application/x-www-form-urlencoded";
        httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues> { [key] = value });
        return new RouteContext(httpContext);
    }
}
