namespace Smart.AspNetCore.Filters
{
    public class TimeLoggingOptions
    {
        public string Key { get; set; } = "_TimeLogging";

        public long Thresold { get; set; }
    }
}
