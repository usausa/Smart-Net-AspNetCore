namespace Smart.AspNetCore.Middleware;

#pragma warning disable CA1819
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
#pragma warning restore CA1819
