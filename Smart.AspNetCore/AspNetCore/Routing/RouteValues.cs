namespace Smart.AspNetCore.Routing;

using System.Collections.Concurrent;
using System.Reflection;

using Microsoft.AspNetCore.Routing;

using Smart.Reflection;

public static class RouteValues
{
    private static readonly ConcurrentDictionary<Type, Accessor[]> Cache = new();

    public static RouteValueDictionary From(object value)
    {
        var type = value.GetType();
        if (!Cache.TryGetValue(type, out var accessors))
        {
            accessors = CreateAccessors(type);
            Cache[type] = accessors;
        }

        var values = new RouteValueDictionary();
        for (var i = 0; i < accessors.Length; i++)
        {
            var accessor = accessors[i];
            values.Add(accessor.Name, accessor.Getter(value));
        }

        return values;
    }

    public static RouteValueDictionary From(string path, object value)
    {
        var type = value.GetType();
        if (!Cache.TryGetValue(type, out var accessors))
        {
            accessors = CreateAccessors(type);
            Cache[type] = accessors;
        }

        var values = new RouteValueDictionary();
        for (var i = 0; i < accessors.Length; i++)
        {
            var accessor = accessors[i];
            values.Add(path + "." + accessor.Name, accessor.Getter(value));
        }

        return values;
    }

    private static Accessor[] CreateAccessors(Type type)
    {
        var factory = DelegateFactory.Default;

        var accessors = new List<Accessor>();
        foreach (var pi in type.GetRuntimeProperties().Where(IsTargetProperty))
        {
            var getter = factory.CreateGetter(pi);
            if (getter is null)
            {
                continue;
            }

            var converter = pi.GetCustomAttribute<ConvertAttribute>();
            if (converter != null)
            {
                var g = getter;
                getter = x => converter.Convert(g(x));
            }

            accessors.Add(new Accessor
            {
                Name = pi.Name,
                Getter = getter
            });
        }

        return [.. accessors];
    }

    private static bool IsTargetProperty(PropertyInfo pi)
    {
        return pi.GetMethod != null &&
               pi.GetMethod.IsPublic &&
               !pi.GetMethod.IsStatic &&
               pi.GetMethod.GetParameters().Length == 0;
    }

#pragma warning disable SA1401
    private sealed class Accessor
    {
        public string Name = default!;

        public Func<object?, object?> Getter = default!;
    }
#pragma warning restore SA1401
}
