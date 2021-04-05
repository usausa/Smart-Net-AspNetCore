namespace Smart.AspNetCore.Filters
{
    public sealed class TimeLoggingOptions
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string Key { get; set; } = "_TimeLogging";

        public long Threshold { get; set; }
    }
}
