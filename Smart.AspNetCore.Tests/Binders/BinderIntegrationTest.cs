namespace Smart.AspNetCore.Binders;

using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// -----------------------------------------------------------------------
// Binder
// -----------------------------------------------------------------------

internal static partial class HttpRequestBinder
{
    [Bind]
    public static partial SearchQueryRequest BindSearch(IQueryCollection query);

    [Bind]
    public static partial SearchQueryRequest BindSearchFromForm(IFormCollection form);
}

// -----------------------------------------------------------------------
// Helper
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
                var form = await ctx.Request.ReadFormAsync().ConfigureAwait(false);
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
// Test
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
        // Arrange
        var ct = TestContext.Current.CancellationToken;

        // Act
        var response = await client.GetAsync(new Uri("/search?Keyword=hello&Page=2&PageSize=50", UriKind.Relative), ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        // Assert
        Assert.Equal("hello", json.GetProperty("keyword").GetString());
        Assert.Equal(2, json.GetProperty("page").GetInt32());
        Assert.Equal(50, json.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task WhenQueryStringIsMissingThenDefaultValuesAreReturned()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;

        // Act
        var response = await client.GetAsync(new Uri("/search", UriKind.Relative), ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        // Assert
        Assert.Null(json.GetProperty("keyword").GetString());
        Assert.Equal(0, json.GetProperty("page").GetInt32());
        Assert.Equal(0, json.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task WhenFormDataIsPostedThenPropertiesAreBindCorrectly()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Keyword", "form-test"),
            new KeyValuePair<string, string>("Page", "4"),
            new KeyValuePair<string, string>("PageSize", "25")
        });

        // Act
        var response = await client.PostAsync(new Uri("/search-form", UriKind.Relative), form, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        // Assert
        Assert.Equal("form-test", json.GetProperty("keyword").GetString());
        Assert.Equal(4, json.GetProperty("page").GetInt32());
        Assert.Equal(25, json.GetProperty("pageSize").GetInt32());
    }
}
