namespace Smart.AspNetCore.Filters
{
    public sealed class TimeLoggingOptions
    {
        public string Key { get; set; } = "_TimeLogging";

        public string Message { get; set; } = "Long execution. Elapsed=[{0}]";

        public long Threshold { get; set; }
    }
}
