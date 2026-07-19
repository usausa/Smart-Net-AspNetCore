namespace Smart.AspNetCore.Middleware;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smart.AspNetCore.Logging;

public sealed class RequestResponseDumpMiddlewareTest
{
    private static WebApplicationBuilder CreateBuilder(LogLevel minimumLevel, ListLoggerProvider logs)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(logs);
        builder.Logging.SetMinimumLevel(minimumLevel);
        return builder;
    }

    [Fact]
    public async Task WhenDebugEnabledThenResponseBodyIsPreservedAndDumped()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var logs = new ListLoggerProvider();
        await using var app = CreateBuilder(LogLevel.Debug, logs).Build();
        app.UseRequestResponseDump();
        app.MapPost("/echo", static () => Results.Json(new { Message = "ok" }));
        await app.StartAsync(ct);
        using var client = app.GetTestClient();

        // Act
        var response = await client.PostAsJsonAsync(new Uri("/echo", UriKind.Relative), new { Value = "hello" }, ct);

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        Assert.Equal("ok", json.GetProperty("message").GetString());
        Assert.Contains(logs.Entries, static x => x.Message.Contains("Request dump", StringComparison.Ordinal));
        Assert.Contains(logs.Entries, static x => x.Message.Contains("Response dump", StringComparison.Ordinal));
    }

    [Fact]
    public async Task WhenDebugDisabledThenResponseBodyIsPreservedAndNotDumped()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var logs = new ListLoggerProvider();
        await using var app = CreateBuilder(LogLevel.Information, logs).Build();
        app.UseRequestResponseDump();
        app.MapPost("/echo", static () => Results.Json(new { Message = "ok" }));
        await app.StartAsync(ct);
        using var client = app.GetTestClient();

        // Act
        var response = await client.PostAsJsonAsync(new Uri("/echo", UriKind.Relative), new { Value = "hello" }, ct);

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        Assert.Equal("ok", json.GetProperty("message").GetString());
        Assert.DoesNotContain(logs.Entries, static x => x.Message.Contains("dump", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task WhenNextThrowsThenResponseBodyIsRestoredForExceptionHandler()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var logs = new ListLoggerProvider();
        await using var app = CreateBuilder(LogLevel.Debug, logs).Build();
        app.UseExceptionHandler(static errorApp =>
        {
            errorApp.Run(static context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\":\"handled\"}", context.RequestAborted);
            });
        });
        app.UseRequestResponseDump();
        app.MapGet("/throw", (Action)(static () => throw new InvalidOperationException("boom")));
        await app.StartAsync(ct);
        using var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync(new Uri("/throw", UriKind.Relative), ct);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        Assert.Equal("handled", json.GetProperty("error").GetString());
    }

    [Fact]
    public async Task WhenResponseExceedsMaxDumpBytesThenFullBodyIsSentAndDumpIsTruncated()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var logs = new ListLoggerProvider();
        var payload = new string('A', 500) + "END";
        var builder = CreateBuilder(LogLevel.Debug, logs);
        builder.Services.Configure<RequestResponseDumpOptions>(static o => o.MaxDumpBytes = 16);
        await using var app = builder.Build();
        app.UseRequestResponseDump();
        app.MapGet("/big", () => Results.Text(payload, "application/json"));
        await app.StartAsync(ct);
        using var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync(new Uri("/big", UriKind.Relative), ct);

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(ct);
        Assert.Equal(payload, body);
        Assert.Contains(logs.Entries, static x => x.Message.Contains("Response dump", StringComparison.Ordinal));
        Assert.DoesNotContain(logs.Entries, static x => x.Message.Contains("END", StringComparison.Ordinal));
    }

    [Fact]
    public async Task WhenResponseContentTypeIsNotTargetThenResponseIsNotDumped()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var logs = new ListLoggerProvider();
        await using var app = CreateBuilder(LogLevel.Debug, logs).Build();
        app.UseRequestResponseDump();
        app.MapGet("/text", () => Results.Text("plain text", "text/plain"));
        await app.StartAsync(ct);
        using var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync(new Uri("/text", UriKind.Relative), ct);

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(ct);
        Assert.Equal("plain text", body);
        Assert.DoesNotContain(logs.Entries, static x => x.Message.Contains("Response dump", StringComparison.Ordinal));
    }
}
