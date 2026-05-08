namespace Smart.AspNetCore.Middleware;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class RequestResponseDumpMiddleware
{
    private static readonly Encoding TextEncoding = Encoding.UTF8;

    private readonly RequestDelegate next;

    private readonly ILogger<RequestResponseDumpMiddleware> log;

    private readonly RequestResponseDumpOptions options;

    public RequestResponseDumpMiddleware(RequestDelegate next, ILogger<RequestResponseDumpMiddleware> log, IOptions<RequestResponseDumpOptions> options)
    {
        this.next = next;
        this.log = log;
        this.options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        if (log.IsEnabled(LogLevel.Debug))
        {
            if (IsDumpTarget(context.Request.ContentType))
            {
                var requestBody = await ReadRequestBodyAsync(context.Request).ConfigureAwait(false);
                if (requestBody.Length > 0)
                {
                    log.DebugRequestDump(TextEncoding.GetString(requestBody.AsSpan(0, Math.Min(requestBody.Length, options.MaxDumpBytes))));
                }
            }

            var originalBodyStream = context.Response.Body;
            await using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await next(context).ConfigureAwait(false);

            if (IsDumpTarget(context.Response.ContentType))
            {
                var responseBody = responseBodyStream.ToArray();
                if (responseBody.Length > 0)
                {
                    log.DebugResponseDump(TextEncoding.GetString(responseBody.AsSpan(0, Math.Min(responseBody.Length, options.MaxDumpBytes))));
                }
            }

            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalBodyStream).ConfigureAwait(false);
        }
        else
        {
            await next(context).ConfigureAwait(false);
        }
    }

    private static async ValueTask<byte[]> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        await using var memoryStream = new MemoryStream((int)(request.ContentLength ?? 0));
        await request.Body.CopyToAsync(memoryStream).ConfigureAwait(false);

        request.Body.Seek(0, SeekOrigin.Begin);

        return memoryStream.ToArray();
    }

    private bool IsDumpTarget(string? contentType)
    {
        if (String.IsNullOrEmpty(contentType))
        {
            return false;
        }

        foreach (var targetType in options.TargetTypes)
        {
            if (contentType.StartsWith(targetType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
