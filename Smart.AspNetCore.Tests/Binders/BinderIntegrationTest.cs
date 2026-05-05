namespace Smart.AspNetCore.Binders;

using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// -----------------------------------------------------------------------
// Web app binder (uses ASP.NET Core Minimal API)
// -----------------------------------------------------------------------

internal static partial class HttpRequestBinder
{
    [Bind]
    public static partial SearchQueryRequest BindSearch(IQueryCollection query);

    [Bind]
    public static partial SearchQueryRequest BindSearchFromForm(IFormCollection form);
}

// -----------------------------------------------------------------------
// -----------------------------------------------------------------------
// Test helpers
// -----------------------------------------------------------------------

internal static class TestWebAppFactory
{
    public static HttpClient CreateClient()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddRouting();
        builder.WebHost.UseTestServer();

        var app = builder.Build();
        app.UseRouting();

        app.MapGet("/search", (HttpContext ctx) =>
        {
            var request = HttpRequestBinder.BindSearch(ctx.Request.Query);
            return Results.Ok(new
            {
                request.Keyword,
                request.Page,
                request.PageSize
            });
        });

        app.MapPost("/search-form", async (HttpContext ctx) =>
        {
            var form = await ctx.Request.ReadFormAsync();
            var request = HttpRequestBinder.BindSearchFromForm(form);
            return Results.Ok(new
            {
                request.Keyword,
                request.Page,
                request.PageSize
            });
        });

        app.Start();

        return app.GetTestClient();
    }
}

// -----------------------------------------------------------------------
// Integration tests
// -----------------------------------------------------------------------

public sealed class BinderIntegrationTest : IDisposable
{
    private readonly HttpClient client = TestWebAppFactory.CreateClient();

    public void Dispose()
    {
        client.Dispose();
    }

    [Fact]
    public async Task WhenQueryStringIsPassedThenPropertiesAreBindCorrectly()
    {
        var response = await client.GetAsync("/search?Keyword=hello&Page=2&PageSize=50");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("hello", json.GetProperty("keyword").GetString());
        Assert.Equal(2, json.GetProperty("page").GetInt32());
        Assert.Equal(50, json.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task WhenQueryStringIsMissingThenDefaultValuesAreReturned()
    {
        var response = await client.GetAsync("/search");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Null(json.GetProperty("keyword").GetString());
        Assert.Equal(0, json.GetProperty("page").GetInt32());
        Assert.Equal(0, json.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task WhenFormDataIsPostedThenPropertiesAreBindCorrectly()
    {
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Keyword", "form-test"),
            new KeyValuePair<string, string>("Page", "4"),
            new KeyValuePair<string, string>("PageSize", "25")
        });

        var response = await client.PostAsync("/search-form", form);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("form-test", json.GetProperty("keyword").GetString());
        Assert.Equal(4, json.GetProperty("page").GetInt32());
        Assert.Equal(25, json.GetProperty("pageSize").GetInt32());
    }
}
