namespace Smart.AspNetCore;

using System;

using Microsoft.AspNetCore.Builder;

using Smart.AspNetCore.Http;

public static class ApplicationBuilderExtensions
{
    //--------------------------------------------------------------------------------
    // Decompress
    //--------------------------------------------------------------------------------

    public static IApplicationBuilder UseRequestDecompress(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestDecompressMiddleware>();
    }

    //--------------------------------------------------------------------------------
    // Dump
    //--------------------------------------------------------------------------------

    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<RequestResponseLoggingMiddleware>(new RequestResponseLoggingOption());
        return builder;
    }

    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder, Action<RequestResponseLoggingOption> config)
    {
        var option = new RequestResponseLoggingOption();
        config(option);
        builder.UseMiddleware<RequestResponseLoggingMiddleware>(option);
        return builder;
    }
}
