namespace Smart.AspNetCore.Filters;

public sealed class TimeLoggingOptions
{
    public string Key { get; set; } = "_TimeLogging";

    public long Threshold { get; set; }

    public TimeLoggingHeaderType HeaderType { get; set; } = TimeLoggingHeaderType.None;

    public string Header { get; set; } = "X-Server-Elapsed";
}
