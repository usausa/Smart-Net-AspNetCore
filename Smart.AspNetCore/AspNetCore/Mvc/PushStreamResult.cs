namespace Smart.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

public sealed class PushStreamResult : FileResult
{
    private readonly Func<Stream, Task> callback;

    private readonly string filename;

    public PushStreamResult(string contentType, string filename, Func<Stream, Task> callback)
        : base(contentType)
    {
        this.callback = callback;
        this.filename = filename;
    }

    public override Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        var contentDisposition = new ContentDispositionHeaderValue("attachment");
        contentDisposition.SetHttpFileName(filename);
        response.Headers.ContentDisposition = contentDisposition.ToString();
        response.ContentType = ContentType;
        return callback(context.HttpContext.Response.Body);
    }
}
