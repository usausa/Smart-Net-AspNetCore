namespace Smart.AspNetCore;

using System.Collections.Generic;
#if NETCOREAPP3_1
using System.Linq;
#endif

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

public static class QueryStringExtensions
{
    public static QueryString Replace(this QueryString query, string name, StringValues value)
    {
        var values = QueryHelpers.ParseQuery(query.ToString());
        values[name] = value;

#if NETCOREAPP3_1
        return new QueryBuilder(values.SelectMany(kvp => kvp.Value, (kvp, v) => KeyValuePair.Create(kvp.Key, v))).ToQueryString();
#else
        return new QueryBuilder(values).ToQueryString();
#endif
    }

    public static QueryString Replace(this QueryString query, IDictionary<string, StringValues> dictionary)
    {
        var values = QueryHelpers.ParseQuery(query.ToString());
        foreach (var pair in dictionary)
        {
            values[pair.Key] = pair.Value;
        }

#if NETCOREAPP3_1
        return new QueryBuilder(values.SelectMany(kvp => kvp.Value, (kvp, v) => KeyValuePair.Create(kvp.Key, v))).ToQueryString();
#else
        return new QueryBuilder(values).ToQueryString();
#endif
    }
}
