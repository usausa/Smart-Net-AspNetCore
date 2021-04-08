namespace Smart.AspNetCore.Http
{
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Extensions.Logging;

    public class DumpLoggerBuilder
    {
        private LogLevel logLevel = LogLevel.Debug;

        private DumpType defaultDumpType = DumpType.None;

        private readonly List<DumpTarget> targets = new();

        private Encoding encoding = Encoding.UTF8;

        private DumpLoggerBuilder()
        {
        }

        public static DumpLoggerBuilder Make() => new();

        public static DumpLoggerBuilder MakeDefault() => new DumpLoggerBuilder()
            .AddContentType("application/json")
            .AddContentType("text/json")
            .AddContentType("application/xml")
            .AddContentType("text/xml");

        public DumpLoggerBuilder SetLogLevel(LogLevel value)
        {
            logLevel = value;
            return this;
        }

        public DumpLoggerBuilder SetDefaultDumpType(bool isText)
        {
            defaultDumpType = isText ? DumpType.Text : DumpType.Binary;
            return this;
        }

        public DumpLoggerBuilder AddContentType(string contentType, bool isText = true)
        {
            targets.Add(new DumpTarget { ContentType = contentType, DumpType = isText ? DumpType.Text : DumpType.Binary });
            return this;
        }

        public DumpLoggerBuilder SetTextEncoding(Encoding value)
        {
            encoding = value;
            return this;
        }

        public DefaultDumpLogger Build() => new(logLevel, defaultDumpType, targets.ToArray(), encoding);
    }
}
