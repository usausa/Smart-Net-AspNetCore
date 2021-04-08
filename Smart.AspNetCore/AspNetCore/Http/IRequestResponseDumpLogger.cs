namespace Smart.AspNetCore.Http
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public interface IRequestResponseDumpLogger
    {
        void DumpRequest(ILogger logger, HttpContext context, byte[] body);

        void DumpResponse(ILogger logger, HttpContext context, byte[] body);
    }
}
