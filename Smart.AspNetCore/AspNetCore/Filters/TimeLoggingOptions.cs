namespace Smart.AspNetCore.Filters;

public sealed class TimeLoggingOptions
{
    public string Key { get; set; } = "_TimeLogging";

    public string Message { get; set; } = "Long execution. Elapsed=[{0}]";

    public long Threshold { get; set; }

    public TimeLoggingHeaderType HeaderType { get; set; } = TimeLoggingHeaderType.None;

    public string Header { get; set; } = "X-Server-Elapsed";
}
