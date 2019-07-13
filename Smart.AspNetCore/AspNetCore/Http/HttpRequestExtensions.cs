namespace Smart.AspNetCore.Http
{
    using System;

    using Microsoft.AspNetCore.Http;

    public static class HttpRequestExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            return String.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal) ||
                   String.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);
        }
    }
}
