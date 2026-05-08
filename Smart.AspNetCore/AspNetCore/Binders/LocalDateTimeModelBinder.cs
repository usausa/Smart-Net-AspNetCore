namespace Smart.AspNetCore.Binders;

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.ModelBinding;

public sealed class LocalDateTimeModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (value != ValueProviderResult.None)
        {
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);
            if (DateTime.TryParse(value.FirstValue, out var date))
            {
                bindingContext.Result = ModelBindingResult.Success(DateTime.SpecifyKind(date, DateTimeKind.Local));
            }
        }

        return Task.CompletedTask;
    }
}

public sealed class LocalDateTimeModelBinderProvider : IModelBinderProvider
{
    private static readonly LocalDateTimeModelBinder Binder = new();

    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?)
            ? Binder
            : null;
}
