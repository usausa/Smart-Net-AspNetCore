namespace Smart.AspNetCore.ActionConstraints;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

public sealed class QueryParameterAttributeTest
{
    [Fact]
    public void MatchesWhenQueryValueIsInAllowedSet()
    {
        var attribute = new QueryParameterAttribute("mode", "a", "b");

        Assert.True(attribute.IsValidForRequest(CreateRouteContext("?mode=b"), new ActionDescriptor()));
    }

    [Fact]
    public void DoesNotMatchWhenQueryValueIsNotInAllowedSet()
    {
        var attribute = new QueryParameterAttribute("mode", "a", "b");

        Assert.False(attribute.IsValidForRequest(CreateRouteContext("?mode=c"), new ActionDescriptor()));
    }

    [Fact]
    public void DoesNotMatchWhenQueryKeyIsMissing()
    {
        var attribute = new QueryParameterAttribute("mode", "a");

        Assert.False(attribute.IsValidForRequest(CreateRouteContext("?other=a"), new ActionDescriptor()));
    }

    [Fact]
    public void NullValueIsHandledWithoutThrowing()
    {
        var attribute = new QueryParameterAttribute("mode", [null!]);

        Assert.False(attribute.IsValidForRequest(CreateRouteContext("?mode=a"), new ActionDescriptor()));
    }

    private static RouteContext CreateRouteContext(string queryString)
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                QueryString = new QueryString(queryString)
            }
        };
        return new RouteContext(httpContext);
    }
}
