namespace Smart.AspNetCore.Razor
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc.Razor;

    public sealed class SubAreaViewLocationExpander : IViewLocationExpander
    {
        private readonly bool useSubAreaPath;

        public SubAreaViewLocationExpander()
            : this(false)
        {
        }

        public SubAreaViewLocationExpander(bool useSubAreaPath)
        {
            this.useSubAreaPath = useSubAreaPath;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Ignore")]
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.ActionContext.RouteData.Values.ContainsKey("subarea"))
            {
                var subArea = RazorViewEngine.GetNormalizedRouteValue(context.ActionContext, "subarea");

                if (useSubAreaPath)
                {
                    return viewLocations.Prepend("/Areas/{2}/SubArea/" + subArea + "/Views/{1}/{0}.cshtml");
                }

                return viewLocations.Prepend("/Areas/{2}/" + subArea + "/Views/{1}/{0}.cshtml");
            }

            return viewLocations;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Ignore")]
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            context.ActionContext.ActionDescriptor.RouteValues.TryGetValue("subarea", out var subArea);
            context.Values["subarea"] = subArea;
        }
    }
}
