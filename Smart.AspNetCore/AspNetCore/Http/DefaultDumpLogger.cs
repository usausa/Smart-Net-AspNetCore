namespace Smart.AspNetCore.Http
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Smart.Text;

    public class DefaultDumpLogger : IRequestResponseDumpLogger
    {
        private readonly LogLevel logLevel;

        private readonly DumpType defaultDumpType;

        private readonly DumpTarget[] targets;

        private readonly Encoding textEncoding;

        internal DefaultDumpLogger(LogLevel logLevel, DumpType defaultDumpType, DumpTarget[] targets, Encoding textEncoding)
        {
            this.logLevel = logLevel;
            this.defaultDumpType = defaultDumpType;
            this.targets = targets;
            this.textEncoding = textEncoding;
        }

        public void DumpRequest(ILogger logger, HttpContext context, byte[] body)
        {
            var dumpType = FindEntry(context.Request.ContentType);
            if (dumpType == DumpType.None)
            {
                return;
            }

            if (body.Length == 0)
            {
                logger.Log(logLevel, "Request dump. dump=[]");
                return;
            }

            logger.Log(logLevel, "Request dump. dump=[{dump}]", dumpType == DumpType.Text ? textEncoding.GetString(body) : HexEncoder.ToHex(body));
        }

        public void DumpResponse(ILogger logger, HttpContext context, byte[] body)
        {
            var dumpType = FindEntry(context.Response.ContentType);
            if (dumpType == DumpType.None)
            {
                return;
            }

            if (body.Length == 0)
            {
                logger.Log(logLevel, "Response dump. dump=[]");
                return;
            }

            logger.Log(logLevel, "Response dump. dump=[{dump}]", dumpType == DumpType.Text ? textEncoding.GetString(body) : HexEncoder.ToHex(body));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DumpType FindEntry(string? contentType)
        {
            if (String.IsNullOrEmpty(contentType))
            {
                return DumpType.None;
            }

            for (var i = 0; i < targets.Length; i++)
            {
                var target = targets[i];
                if (contentType.StartsWith(target.ContentType, StringComparison.InvariantCulture))
                {
                    return target.DumpType;
                }
            }

            return defaultDumpType;
        }
    }
}
