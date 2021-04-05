namespace Smart.AspNetCore
{
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc;

    using Smart.AspNetCore.Http;

    public static class UrlHelperExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static HtmlString ReplaceQuery(this IUrlHelper helper, string key, string value)
        {
            return new HtmlString(helper.ActionContext.HttpContext.Request.Query.Replace(key, value));
        }
    }
}
