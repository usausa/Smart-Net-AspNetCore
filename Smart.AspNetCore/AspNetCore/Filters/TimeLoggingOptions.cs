namespace Smart.AspNetCore.Filters
{
    public sealed class TimeLoggingOptions
    {
        public string Key { get; set; } = "_TimeLogging";

        public long Thresold { get; set; }
    }
}
