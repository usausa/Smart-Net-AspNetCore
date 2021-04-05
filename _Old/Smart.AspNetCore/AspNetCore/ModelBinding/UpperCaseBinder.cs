namespace Smart.AspNetCore.ModelBinding
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public sealed class UpperCaseBinder : IModelBinder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Ignore")]
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProvider = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProvider == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(valueProvider.FirstValue.ToUpperInvariant());

            return Task.CompletedTask;
        }
    }
}
