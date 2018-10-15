namespace Smart.AspNetCore
{
    using System;

    using Microsoft.AspNetCore.Http;

    public static class HttpRequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            return String.Equals(request.Query["X-Requested-With"], "XMLHttpRequest") ||
                   String.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest");
        }
    }
}
