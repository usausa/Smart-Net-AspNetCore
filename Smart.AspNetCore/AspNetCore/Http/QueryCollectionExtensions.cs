namespace Smart.AspNetCore.Http
{
    using System;
    using System.Net;
    using System.Text;

    using Microsoft.AspNetCore.Http;

    public static class QueryCollectionExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static string Replace(this IQueryCollection query, string key, string value)
        {
            var replaced = false;
            var sb = new StringBuilder();

            foreach (var index in query.Keys)
            {
                if (String.Equals(index, key, StringComparison.OrdinalIgnoreCase))
                {
                    if (!replaced)
                    {
                        AddParameter(sb, key, value);
                        replaced = true;
                    }
                }
                else
                {
                    foreach (var str in query[index])
                    {
                        AddParameter(sb, index, str);
                    }
                }
            }

            if (!replaced)
            {
                AddParameter(sb, key, value);
            }

            return sb.ToString();
        }

        private static void AddParameter(StringBuilder sb, string key, string value)
        {
            sb.Append(sb.Length == 0 ? "?" : "&");
            sb.Append(WebUtility.UrlEncode(key));
            sb.Append("=");
            sb.Append(WebUtility.UrlEncode(value));
        }
    }
}
