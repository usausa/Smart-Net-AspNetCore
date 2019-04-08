namespace Smart.AspNetCore.ActionConstraints
{
    using System;

    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Routing;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SubmitParameterAttribute : ActionMethodSelectorAttribute
    {
        private readonly string name;

        private readonly string value;

        public SubmitParameterAttribute(string name)
            : this(name, string.Empty)
        {
        }

        public SubmitParameterAttribute(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

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
