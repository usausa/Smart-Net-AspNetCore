namespace Smart.AspNetCore.Filters
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public sealed class NotFoundResultFilterAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value == null)
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}
