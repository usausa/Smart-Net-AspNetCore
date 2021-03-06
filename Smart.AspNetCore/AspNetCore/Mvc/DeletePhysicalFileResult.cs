namespace Smart.AspNetCore.Mvc
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    public class DeletePhysicalFileResult : PhysicalFileResult
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
}
