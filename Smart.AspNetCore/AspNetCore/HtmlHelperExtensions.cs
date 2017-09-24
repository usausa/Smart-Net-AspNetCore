namespace Smart.AspNetCore
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc.Rendering;

    /// <summary>
    ///
    /// </summary>
    public static class HtmlHelperExtensions
    {
        public static bool IsSelected(this IHtmlHelper helper, params string[] controllers)
        {
            var currentController = (string)helper.ViewContext.RouteData.Values["controller"];
            return controllers.Any(x => x.Equals(currentController, StringComparison.OrdinalIgnoreCase));
        }

        public static bool HasError(this IHtmlHelper helper, string key)
        {
            return helper.ViewData.ModelState.TryGetValue(key, out var entry) && entry.Errors.Count > 0;
        }
    }
}
