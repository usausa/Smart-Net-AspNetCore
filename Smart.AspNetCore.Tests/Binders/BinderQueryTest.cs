namespace Smart.AspNetCore.Binders;

using Microsoft.AspNetCore.Http;

using Smart.AspNetCore.Binders;

// -----------------------------------------------------------------------
// Binders using ASP.NET Core collections
// -----------------------------------------------------------------------

internal static partial class QueryBinder
{
    [Bind]
    public static partial SearchQueryRequest BindFromQuery(IQueryCollection query);

    [Bind]
    public static partial void BindFromQueryToInstance(IQueryCollection query, SearchQueryRequest target);
}

internal static partial class FormBinder
{
    [Bind]
    public static partial SearchQueryRequest BindFromForm(IFormCollection form);
}

internal sealed class SearchQueryRequest
{
    public string? Keyword { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

// -----------------------------------------------------------------------
// Tests
// -----------------------------------------------------------------------

public sealed class BinderQueryTest
{
    [Fact]
    public void WhenBindingFromIQueryCollectionThenPropertiesAreMapped()
    {
        var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["Keyword"] = "hello",
            ["Page"] = "2",
            ["PageSize"] = "50",
        });

        var result = QueryBinder.BindFromQuery(query);

        Assert.Equal("hello", result.Keyword);
        Assert.Equal(2, result.Page);
        Assert.Equal(50, result.PageSize);
    }

    [Fact]
    public void WhenBindingFromIQueryCollectionWithMissingValuesThenDefaultsAreUsed()
    {
        var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

        var result = QueryBinder.BindFromQuery(query);

        Assert.Null(result.Keyword);
        Assert.Equal(0, result.Page);
        Assert.Equal(0, result.PageSize);
    }

    [Fact]
    public void WhenBindingFromIQueryCollectionToInstanceThenPropertiesAreMappedAndExistingValuesPreserved()
    {
        var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["Keyword"] = "world",
            ["Page"] = "3",
        });

        var target = new SearchQueryRequest { PageSize = 20 };
        QueryBinder.BindFromQueryToInstance(query, target);

        Assert.Equal("world", target.Keyword);
        Assert.Equal(3, target.Page);
        Assert.Equal(20, target.PageSize); // not in query, default preserved
    }

    [Fact]
    public void WhenBindingFromIFormCollectionThenPropertiesAreMapped()
    {
        var form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["Keyword"] = "form-test",
            ["Page"] = "4",
            ["PageSize"] = "25",
        });

        var result = FormBinder.BindFromForm(form);

        Assert.Equal("form-test", result.Keyword);
        Assert.Equal(4, result.Page);
        Assert.Equal(25, result.PageSize);
    }
}
