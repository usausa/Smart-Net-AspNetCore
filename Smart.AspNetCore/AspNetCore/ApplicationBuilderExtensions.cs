namespace Smart.AspNetCore;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using Smart.AspNetCore.Middleware;

public static class ApplicationBuilderExtensions
{
    //--------------------------------------------------------------------------------
    // UseWhen
    //--------------------------------------------------------------------------------

    public static IApplicationBuilder UseWhen(
        this IApplicationBuilder app,
        Func<HttpContext, bool> predicate,
        Action<IApplicationBuilder> whenConfiguration,
        Action<IApplicationBuilder> elseConfiguration)
    {
        var whenBranchBuilder = app.New();
        whenConfiguration(whenBranchBuilder);

        var elseBranchBuilder = app.New();
        elseConfiguration(elseBranchBuilder);

        return app.Use(main =>
        {
            whenBranchBuilder.Run(main);
            var whenBranch = whenBranchBuilder.Build();

            elseBranchBuilder.Run(main);
            var elseBranch = elseBranchBuilder.Build();

            return context => predicate(context) ? whenBranch(context) : elseBranch(context);
        });
    }

    //--------------------------------------------------------------------------------
    // Request/Response dump
    //--------------------------------------------------------------------------------

    public static IApplicationBuilder UseRequestResponseDump(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestResponseDumpMiddleware>();
}
