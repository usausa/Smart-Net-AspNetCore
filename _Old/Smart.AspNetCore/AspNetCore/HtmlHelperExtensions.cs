namespace Smart.AspNetCore
{
    using System;
    using System.Linq;
    using System.Threading;

    using Microsoft.AspNetCore.Mvc.Rendering;

    public static class HtmlHelperExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static bool IsSelected(this IHtmlHelper helper, params string[] controllers)
        {
            var currentController = (string)helper.ViewContext.RouteData.Values["controller"];
            return controllers.Any(x => x.Equals(currentController, StringComparison.OrdinalIgnoreCase));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static bool HasError(this IHtmlHelper helper)
        {
            return helper.ViewData.ModelState.ErrorCount > 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static bool HasError(this IHtmlHelper helper, string key)
        {
            return helper.ViewData.ModelState.TryGetValue(key, out var entry) && entry.Errors.Count > 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static bool HasRouteData(this IHtmlHelper helper, string key)
        {
            return helper.ViewContext.RouteData.Values.ContainsKey(key);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static T RouteValue<T>(this IHtmlHelper helper, string key)
        {
            return (T)Convert.ChangeType(helper.ViewContext.RouteData.Values[key], typeof(T), Thread.CurrentThread.CurrentCulture);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static string QueryValue(this IHtmlHelper helper, string key)
        {
            return helper.ViewContext.HttpContext.Request.Query[key].FirstOrDefault();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static IEquatable<string> QueryValues(this IHtmlHelper helper, string key)
        {
            return helper.ViewContext.HttpContext.Request.Query[key];
        }
    }
}
