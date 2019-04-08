namespace Smart.AspNetCore.TagHelpers
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.TagHelpers;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;

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

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ViewContext.ViewData.ModelState.TryGetValue(For.Name, out var entry);

            if (entry is null || (entry.Errors.Count == 0))
            {
                return;
            }

            var tag = new TagBuilder("div");
            tag.AddCssClass(ValidationErrorClass);
            output.MergeAttributes(tag);
        }
    }
}
