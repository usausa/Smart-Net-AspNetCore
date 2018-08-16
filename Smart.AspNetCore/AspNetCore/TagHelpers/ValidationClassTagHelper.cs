namespace Smart.AspNetCore.TagHelpers
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.TagHelpers;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    /// <summary>
    ///
    /// </summary>
    [HtmlTargetElement("div", Attributes = ValidationForAttributeName + "," + ValidationErrorClassName)]
    public sealed class ValidationClassTagHelper : TagHelper
    {
        private const string ValidationForAttributeName = "s-validation-for";

        private const string ValidationErrorClassName = "s-validationerror-class";

        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(ValidationErrorClassName)]
        public string ValidationErrorClass { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ViewContext.ViewData.ModelState.TryGetValue(For.Name, out var entry);

            if ((entry == null) || (entry.Errors.Count == 0))
            {
                return;
            }

            var tag = new TagBuilder("div");
            tag.AddCssClass(ValidationErrorClass);
            output.MergeAttributes(tag);
        }
    }
}
