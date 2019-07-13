namespace Smart.AspNetCore
{
    using Microsoft.AspNetCore.Builder;

    using Smart.AspNetCore.Http;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestDecompress(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestDecompressMiddleware>();
        }
    }
}
