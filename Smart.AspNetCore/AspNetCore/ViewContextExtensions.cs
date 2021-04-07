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

        //--------------------------------------------------------------------------------
        // Route value
        //--------------------------------------------------------------------------------

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

        //--------------------------------------------------------------------------------
        // Query parameter
        //--------------------------------------------------------------------------------

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

        //--------------------------------------------------------------------------------
        // Verb
        //--------------------------------------------------------------------------------

        public static bool IsPost(this ViewContext context)
        {
            return HttpMethods.IsPost(context.HttpContext.Request.Method);
        }

        //--------------------------------------------------------------------------------
        // Controller
        //--------------------------------------------------------------------------------

        public static bool IsAreaActive(this ViewContext context, string area)
        {
            return context.RouteData.Values.TryGetValue("area", out var currentArea) &&
                   area.Equals((string)currentArea!, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsControllerActive(this ViewContext context, string area, string controller)
        {
            return context.RouteData.Values.TryGetValue("area", out var currentArea) &&
                   area.Equals((string)currentArea!, StringComparison.OrdinalIgnoreCase) &&
                   context.RouteData.Values.TryGetValue("controller", out var currentController) &&
                   controller.Equals((string)currentController!, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsControllerActive(this ViewContext context, string area, string[] controllers)
        {
            return context.RouteData.Values.TryGetValue("area", out var currentArea) &&
                   area.Equals((string)currentArea!, StringComparison.OrdinalIgnoreCase) &&
                   context.RouteData.Values.TryGetValue("controller", out var currentController) &&
                   controllers.Any(x => x.Equals((string)currentController!, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsActionActive(this ViewContext context, string area, string controller, string action)
        {
            return context.RouteData.Values.TryGetValue("area", out var currentArea) &&
                   area.Equals((string)currentArea!, StringComparison.OrdinalIgnoreCase) &&
                   context.RouteData.Values.TryGetValue("controller", out var currentController) &&
                   controller.Equals((string)currentController!, StringComparison.OrdinalIgnoreCase) &&
                   context.RouteData.Values.TryGetValue("action", out var currentAction) &&
                   action.Equals((string)currentAction!, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsActionActive(this ViewContext context, string area, string controller, string[] actions)
        {
            return context.RouteData.Values.TryGetValue("area", out var currentArea) &&
                   area.Equals((string)currentArea!, StringComparison.OrdinalIgnoreCase) &&
                   context.RouteData.Values.TryGetValue("controller", out var currentController) &&
                   controller.Equals((string)currentController!, StringComparison.OrdinalIgnoreCase) &&
                   context.RouteData.Values.TryGetValue("action", out var currentAction) &&
                   actions.Any(x => x.Equals((string)currentAction!, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsActionActive(this ViewContext context, string controller, string action)
        {
            return context.RouteData.Values.TryGetValue("controller", out var currentController) &&
                   controller.Equals((string)currentController!, StringComparison.OrdinalIgnoreCase) &&
                   context.RouteData.Values.TryGetValue("action", out var currentAction) &&
                   action.Equals((string)currentAction!, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsActionActive(this ViewContext context, string controller, string[] actions)
        {
            return context.RouteData.Values.TryGetValue("controller", out var currentController) &&
                   controller.Equals((string)currentController!, StringComparison.OrdinalIgnoreCase) &&
                   context.RouteData.Values.TryGetValue("action", out var currentAction) &&
                   actions.Any(x => x.Equals((string)currentAction!, StringComparison.OrdinalIgnoreCase));
        }
    }
}
