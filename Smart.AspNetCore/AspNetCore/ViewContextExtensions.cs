namespace Smart.AspNetCore
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public static class ViewContextExtensions
    {
        [return: MaybeNull]
        private static T TypeConvert<T>(object? value)
        {
            if (value is null)
            {
                return default;
            }

            var type = typeof(T);
            var targetType = Nullable.GetUnderlyingType(type) ?? type;
            if (targetType.IsEnum)
            {
                try
                {
                    return (T)Enum.Parse(targetType, value.ToString()!, true);
                }
                catch (Exception e) when (e is ArgumentException || e is OverflowException)
                {
                    return default;
                }
            }

            try
            {
                return (T)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is FormatException || e is OverflowException)
            {
                return default;
            }
        }

        public static bool HasRouteValue(this ViewContext context, string key)
        {
            return context.RouteData.Values.ContainsKey(key);
        }

        [return: MaybeNull]
        public static T GetRouteValue<T>(this ViewContext context, string key)
        {
            var value = context.RouteData.Values[key];
            return value switch
            {
                null => default,
                T typedValue => typedValue,
                _ => TypeConvert<T>(value)
            };
        }

        public static string? GetQueryString(this ViewContext context, string key)
        {
            return context.HttpContext.Request.Query[key].FirstOrDefault();
        }

        [return: MaybeNull]
        public static T GetQueryValue<T>(this ViewContext context, string key)
        {
            var value = context.HttpContext.Request.Query[key].FirstOrDefault();
            return value switch
            {
                null => default,
                T typedValue => typedValue,
                _ => TypeConvert<T>(value)
            };
        }

        public static bool IsPost(this ViewContext context)
        {
            return HttpMethods.IsPost(context.HttpContext.Request.Method);
        }
    }
}
