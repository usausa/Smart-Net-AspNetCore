namespace Smart.AspNetCore;

using Microsoft.Extensions.Logging;

#pragma warning disable SYSLIB1006
public static partial class Log
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception.")]
    public static partial void ErrorUnhandledException(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Long execution. elapsed=[{elapsed}]")]
    public static partial void WarnLongExecution(this ILogger logger, long elapsed);
}
#pragma warning restore SYSLIB1006
