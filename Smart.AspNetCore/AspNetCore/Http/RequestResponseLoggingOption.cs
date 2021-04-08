namespace Smart.AspNetCore.Http
{
    public class RequestResponseLoggingOption
    {
        public IRequestResponseDumpLogger DumpLogger { get; set; } = DumpLoggerBuilder.MakeDefault().Build();
    }
}
