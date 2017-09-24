namespace Smart.AspNetCore.Filters
{
    using System;

    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Routing;

    /// <summary>
    ///
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SubmitParameterAttribute : ActionMethodSelectorAttribute
    {
        private readonly string name;

        private readonly string value;

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        public SubmitParameterAttribute(string name)
            : this(name, string.Empty)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public SubmitParameterAttribute(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="routeContext"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            if (!routeContext.HttpContext.Request.HasFormContentType)
            {
                return false;
            }

            var formValues = routeContext.HttpContext.Request.Form[name];

            return formValues.Count == 1 &&
                   (String.IsNullOrEmpty(value) ||
                    String.Equals(formValues[0], value, StringComparison.OrdinalIgnoreCase));
        }
    }
}
