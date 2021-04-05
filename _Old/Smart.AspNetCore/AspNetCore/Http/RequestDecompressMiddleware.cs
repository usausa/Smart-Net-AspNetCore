namespace Smart.AspNetCore.Http
{
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public class RequestDecompressMiddleware
    {
        private enum EncodingType
        {
            None,
            Gzip,
            Deflate
        }

        private readonly RequestDelegate next;

        public RequestDecompressMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Ignore")]
        public async Task Invoke(HttpContext context)
        {
            var encodingType = ResolveEncodingType(context.Request.Headers);
            if (encodingType != EncodingType.None)
            {
                var stream = new MemoryStream();

                using (var source = encodingType == EncodingType.Gzip
                    ? (Stream)new GZipStream(context.Request.Body, CompressionMode.Decompress, true)
                    : new DeflateStream(context.Request.Body, CompressionMode.Decompress, true))
                {
                    await source.CopyToAsync(stream).ConfigureAwait(false);
                }

                stream.Seek(0, SeekOrigin.Begin);

                context.Request.Body = stream;
                context.Request.Headers["Content-Length"] = stream.Length.ToString(CultureInfo.InvariantCulture);
            }

            await next(context).ConfigureAwait(false);
        }

        private static EncodingType ResolveEncodingType(IHeaderDictionary header)
        {
            if (header.TryGetValue("Content-Encoding", out var value))
            {
                header.Remove("Content-Encoding");

                if (value == "gzip")
                {
                    return EncodingType.Gzip;
                }

                if (value == "deflate")
                {
                    return EncodingType.Deflate;
                }
            }

            return EncodingType.None;
        }
    }
}
