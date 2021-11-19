namespace Smart.AspNetCore.Http;

using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate next;

    private readonly ILogger<RequestResponseLoggingMiddleware> logger;

    private readonly IRequestResponseDumpLogger dumpLogger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger, RequestResponseLoggingOption option)
    {
        this.next = next;
        this.logger = logger;
        dumpLogger = option.DumpLogger;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestBody = await ReadRequestBody(context.Request).ConfigureAwait(false);
        dumpLogger.DumpRequest(logger, context, requestBody);

        var originalBodyStream = context.Response.Body;
#pragma warning disable CA2007
        await using var responseBodyStream = new MemoryStream();
#pragma warning restore CA2007
        context.Response.Body = responseBodyStream;

        await next(context).ConfigureAwait(false);

        var responseBody = await ReadResponseBody(context.Response).ConfigureAwait(false);
        dumpLogger.DumpResponse(logger, context, responseBody);

        if (responseBody.Length > 0)
        {
            await responseBodyStream.CopyToAsync(originalBodyStream).ConfigureAwait(false);
        }
    }

    private static async ValueTask<byte[]> ReadRequestBody(HttpRequest request)
    {
        request.EnableBuffering();

#pragma warning disable CA2007
        await using var memoryStream = new MemoryStream((int)(request.ContentLength ?? 0));
#pragma warning restore CA2007
        await request.Body.CopyToAsync(memoryStream).ConfigureAwait(false);

        request.Body.Seek(0, SeekOrigin.Begin);

        return memoryStream.ToArray();
    }

    private static async ValueTask<byte[]> ReadResponseBody(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);

#pragma warning disable CA2007
        await using var memoryStream = new MemoryStream();
#pragma warning restore CA2007
        await response.Body.CopyToAsync(memoryStream).ConfigureAwait(false);

        response.Body.Seek(0, SeekOrigin.Begin);

        return memoryStream.ToArray();
    }
}
