namespace Smart.AspNetCore.Filters;

using System.Globalization;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Smart.AspNetCore.Logging;

public sealed class TimeLoggingFilterTest
{
    private const string HeaderName = "X-Server-Elapsed";

    [Fact]
    public void WhenExecutionReachesThresholdThenWarningIsLogged()
    {
        using var logs = new ListLoggerProvider();
        using var loggerFactory = CreateLoggerFactory(logs);
        var filter = new TimeLoggingFilter(
            loggerFactory.CreateLogger<TimeLoggingFilter>(),
            Options.Create(new TimeLoggingOptions { Threshold = 0, HeaderType = TimeLoggingHeaderType.None }));

        var httpContext = new DefaultHttpContext();
        var (executing, executed) = CreateActionContexts(httpContext);

        filter.OnActionExecuting(executing);
        filter.OnActionExecuted(executed);

        Assert.Contains(logs.Entries, static x => x.Level == LogLevel.Warning && x.Message.Contains("Long execution", StringComparison.Ordinal));
        Assert.False(httpContext.Response.Headers.ContainsKey(HeaderName));
    }

    [Fact]
    public void WhenHeaderTypeIsLongExecutionAndExecutionIsLongThenElapsedHeaderIsAdded()
    {
        using var logs = new ListLoggerProvider();
        using var loggerFactory = CreateLoggerFactory(logs);
        var filter = new TimeLoggingFilter(
            loggerFactory.CreateLogger<TimeLoggingFilter>(),
            Options.Create(new TimeLoggingOptions { Threshold = 0, HeaderType = TimeLoggingHeaderType.LongExecution }));

        var httpContext = new DefaultHttpContext();
        var (executing, executed) = CreateActionContexts(httpContext);

        filter.OnActionExecuting(executing);
        filter.OnActionExecuted(executed);

        Assert.Contains(logs.Entries, static x => x.Level == LogLevel.Warning && x.Message.Contains("Long execution", StringComparison.Ordinal));
        AssertNumericHeader(httpContext);
    }

    [Fact]
    public void WhenExecutionIsUnderThresholdThenNoWarningAndNoHeaderForLongExecution()
    {
        using var logs = new ListLoggerProvider();
        using var loggerFactory = CreateLoggerFactory(logs);
        var filter = new TimeLoggingFilter(
            loggerFactory.CreateLogger<TimeLoggingFilter>(),
            Options.Create(new TimeLoggingOptions { Threshold = Int64.MaxValue, HeaderType = TimeLoggingHeaderType.LongExecution }));

        var httpContext = new DefaultHttpContext();
        var (executing, executed) = CreateActionContexts(httpContext);

        filter.OnActionExecuting(executing);
        filter.OnActionExecuted(executed);

        Assert.DoesNotContain(logs.Entries, static x => x.Level == LogLevel.Warning && x.Message.Contains("Long execution", StringComparison.Ordinal));
        Assert.False(httpContext.Response.Headers.ContainsKey(HeaderName));
    }

    [Fact]
    public void WhenHeaderTypeIsAlwaysAndExecutionIsUnderThresholdThenElapsedHeaderIsAdded()
    {
        using var logs = new ListLoggerProvider();
        using var loggerFactory = CreateLoggerFactory(logs);
        var filter = new TimeLoggingFilter(
            loggerFactory.CreateLogger<TimeLoggingFilter>(),
            Options.Create(new TimeLoggingOptions { Threshold = Int64.MaxValue, HeaderType = TimeLoggingHeaderType.Always }));

        var httpContext = new DefaultHttpContext();
        var (executing, executed) = CreateActionContexts(httpContext);

        filter.OnActionExecuting(executing);
        filter.OnActionExecuted(executed);

        Assert.DoesNotContain(logs.Entries, static x => x.Level == LogLevel.Warning && x.Message.Contains("Long execution", StringComparison.Ordinal));
        AssertNumericHeader(httpContext);
    }

    [Fact]
    public void WhenHeaderTypeIsAlwaysAndExecutionIsLongThenElapsedHeaderIsAdded()
    {
        using var logs = new ListLoggerProvider();
        using var loggerFactory = CreateLoggerFactory(logs);
        var filter = new TimeLoggingFilter(
            loggerFactory.CreateLogger<TimeLoggingFilter>(),
            Options.Create(new TimeLoggingOptions { Threshold = 0, HeaderType = TimeLoggingHeaderType.Always }));

        var httpContext = new DefaultHttpContext();
        var (executing, executed) = CreateActionContexts(httpContext);

        filter.OnActionExecuting(executing);
        filter.OnActionExecuted(executed);

        Assert.Contains(logs.Entries, static x => x.Level == LogLevel.Warning && x.Message.Contains("Long execution", StringComparison.Ordinal));
        AssertNumericHeader(httpContext);
    }

    [Fact]
    public void WhenStopwatchIsNotPresentThenNothingIsLoggedOrHeadered()
    {
        using var logs = new ListLoggerProvider();
        using var loggerFactory = CreateLoggerFactory(logs);
        var filter = new TimeLoggingFilter(
            loggerFactory.CreateLogger<TimeLoggingFilter>(),
            Options.Create(new TimeLoggingOptions { Threshold = 0, HeaderType = TimeLoggingHeaderType.Always }));

        var httpContext = new DefaultHttpContext();
        var (_, executed) = CreateActionContexts(httpContext);

        filter.OnActionExecuted(executed);

        Assert.Empty(logs.Entries);
        Assert.False(httpContext.Response.Headers.ContainsKey(HeaderName));
    }

    private static ILoggerFactory CreateLoggerFactory(ListLoggerProvider logs) =>
        LoggerFactory.Create(builder =>
        {
            builder.AddProvider(logs);
            builder.SetMinimumLevel(LogLevel.Debug);
        });

    private static (ActionExecutingContext Executing, ActionExecutedContext Executed) CreateActionContexts(HttpContext httpContext)
    {
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();
        var executing = new ActionExecutingContext(actionContext, filters, new Dictionary<string, object?>(), new object());
        var executed = new ActionExecutedContext(actionContext, filters, new object());
        return (executing, executed);
    }

    private static void AssertNumericHeader(HttpContext httpContext)
    {
        Assert.True(httpContext.Response.Headers.ContainsKey(HeaderName));
        Assert.True(Int64.TryParse(httpContext.Response.Headers[HeaderName].ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out _));
    }
}
