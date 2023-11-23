namespace Smart.AspNetCore.Mvc;

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

public class PushStreamResult : FileResult
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
        response.Headers["Content-Disposition"] = $"attachment; filename={filename}";
        response.ContentType = ContentType;
        return callback(context.HttpContext.Response.Body);
    }
}
