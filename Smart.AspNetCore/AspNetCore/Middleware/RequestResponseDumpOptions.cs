namespace Smart.AspNetCore.Middleware;

public sealed class RequestResponseDumpOptions
{
    public int MaxDumpBytes { get; set; } = 4096;

    public string[] TargetTypes { get; set; } =
    [
        "application/json",
        "text/json",
        "application/xml",
        "text/xml"
    ];
}
