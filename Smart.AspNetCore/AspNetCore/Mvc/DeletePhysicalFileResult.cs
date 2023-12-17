namespace Smart.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

public sealed class DeletePhysicalFileResult : PhysicalFileResult
{
    public DeletePhysicalFileResult(string fileName, string contentType)
        : base(fileName, contentType)
    {
    }

    public DeletePhysicalFileResult(string fileName, MediaTypeHeaderValue contentType)
        : base(fileName, contentType)
    {
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        try
        {
            await base.ExecuteResultAsync(context).ConfigureAwait(false);
        }
        finally
        {
            File.Delete(FileName);
        }
    }
}
