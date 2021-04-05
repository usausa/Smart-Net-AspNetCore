namespace Smart.AspNetCore.TagHelpers
{
    using Microsoft.AspNetCore.Razor.TagHelpers;

    [HtmlTargetElement(Attributes = ConditionAttributeName)]
    public sealed class ConditionTagHelper : TagHelper
    {
        private const string ConditionAttributeName = "s-if";

        [HtmlAttributeName(ConditionAttributeName)]
        public bool Condition { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Ignore")]
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!Condition)
            {
                output.SuppressOutput();
            }
        }
    }
}
