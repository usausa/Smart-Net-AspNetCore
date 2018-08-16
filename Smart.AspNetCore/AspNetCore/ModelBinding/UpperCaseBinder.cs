namespace Smart.AspNetCore.ModelBinding
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    ///
    /// </summary>
    public sealed class UpperCaseBinder : IModelBinder
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
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
