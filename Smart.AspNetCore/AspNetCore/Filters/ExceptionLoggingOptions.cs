namespace Smart.AspNetCore.Filters;

using Microsoft.Extensions.Logging;

public sealed class ExceptionLoggingOptions
{
    public EventId EventId { get; set; }

    public string Message { get; set; } = "Handle exception.";
}
