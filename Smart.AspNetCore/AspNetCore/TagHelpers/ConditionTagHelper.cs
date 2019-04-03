namespace Smart.AspNetCore.TagHelpers
{
    using Microsoft.AspNetCore.Razor.TagHelpers;

    [HtmlTargetElement(Attributes = ConditionAttributeName)]
    public sealed class ConditionTagHelper : TagHelper
    {
        private const string ConditionAttributeName = "s-if";

        [HtmlAttributeName(ConditionAttributeName)]
        public bool Condition { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!Condition)
            {
                output.SuppressOutput();
            }
        }
    }
}
