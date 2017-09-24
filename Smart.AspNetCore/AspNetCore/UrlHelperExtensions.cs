namespace Smart.AspNetCore
{
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc;

    public static class UrlHelperExtensions
    {
        public static HtmlString ReplaceQuery(this IUrlHelper helper, string key, string value)
        {
            return new HtmlString(helper.ActionContext.HttpContext.Request.Query.Replace(key, value));
        }
    }
}
