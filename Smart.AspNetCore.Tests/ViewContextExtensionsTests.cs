namespace Smart.AspNetCore;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

public sealed class ViewContextExtensionsTests
{
    private static ViewContext CreateContext(string query, RouteValueDictionary? routeValues = null)
    {
        var httpContext = new DefaultHttpContext { Request = { QueryString = new QueryString(query) } };
        return new ViewContext { HttpContext = httpContext, RouteData = new RouteData(routeValues ?? new RouteValueDictionary()) };
    }

    [Fact]
    public void GetQueryValueConvertsToTheRequestedType()
    {
        var context = CreateContext("?count=12");

        Assert.Equal(12, context.GetQueryValue<int>("count"));
        Assert.Equal(12, context.GetQueryValue<int?>("count"));
    }

    [Fact]
    public void GetQueryValueParsesAnEnumCaseInsensitively()
    {
        var context = CreateContext("?mode=second");

        Assert.Equal(Mode.Second, context.GetQueryValue<Mode>("mode"));
        Assert.Equal(Mode.Second, context.GetQueryValue<Mode?>("mode"));
    }

    [Fact]
    public void GetQueryValueReturnsDefaultWhenTheValueCannotBeConverted()
    {
        var context = CreateContext("?count=abc&mode=unknown");

        Assert.Equal(0, context.GetQueryValue<int>("count"));
        Assert.Null(context.GetQueryValue<int?>("count"));
        Assert.Equal(Mode.None, context.GetQueryValue<Mode>("mode"));
        Assert.Null(context.GetQueryValue<Mode?>("mode"));
    }

    [Fact]
    public void GetQueryValueReturnsTheValueAsIsWhenItIsAlreadyTheRequestedType()
    {
        var context = CreateContext("?mode=second");

        Assert.Equal("second", context.GetQueryValue<string>("mode"));
    }

    [Fact]
    public void GetRouteValueConvertsToTheRequestedType()
    {
        var context = CreateContext(string.Empty, new RouteValueDictionary { ["id"] = "5", ["mode"] = 2 });

        Assert.Equal(5, context.GetRouteValue<int>("id"));
        Assert.Equal(Mode.Second, context.GetRouteValue<Mode?>("mode"));
    }

    [Fact]
    public void GetRouteValueReturnsDefaultWhenTheKeyIsMissing()
    {
        var context = CreateContext(string.Empty);

        Assert.Equal(0, context.GetRouteValue<int>("id"));
        Assert.Null(context.GetRouteValue<string>("id"));
    }

    public enum Mode
    {
        None,
        First,
        Second
    }
}
