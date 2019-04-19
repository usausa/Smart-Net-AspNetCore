namespace Smart.AspNetCore
{
    using Microsoft.AspNetCore.Builder;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestDecompress(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestDecompressMiddleware>();
        }
    }
}
