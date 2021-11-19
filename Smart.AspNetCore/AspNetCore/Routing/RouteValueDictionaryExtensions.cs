namespace Smart.AspNetCore.Routing;

using Microsoft.AspNetCore.Routing;

public static class RouteValueDictionaryExtensions
{
    public static RouteValueDictionary And(this RouteValueDictionary values, string key, object? value)
    {
        values.Add(key, value ?? string.Empty);
        return values;
    }
}
