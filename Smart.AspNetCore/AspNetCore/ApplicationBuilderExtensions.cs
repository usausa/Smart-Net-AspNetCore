namespace Smart.AspNetCore;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (whenConfiguration == null)
        {
            throw new ArgumentNullException(nameof(whenConfiguration));
        }

        if (elseConfiguration == null)
        {
            throw new ArgumentNullException(nameof(elseConfiguration));
        }

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
}
