namespace Smart.AspNetCore.ModelBinding;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

using Smart.Text.Japanese;

public sealed class KanaConvertModelBinder : IModelBinder
{
    private readonly KanaOption option;

    public KanaConvertModelBinder(KanaOption option)
    {
        this.option = option;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        if (value is null)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(KanaConverter.Convert(value, option));
        return Task.CompletedTask;
    }
}

public sealed class KanaConvertModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType != typeof(string))
        {
            return null;
        }

        var attribute = context.Metadata is DefaultModelMetadata meta
            ? meta.Attributes.PropertyAttributes?.OfType<KanaConvertAttribute>().FirstOrDefault()
            : null;

        if (attribute is null)
        {
            return null;
        }

        return new KanaConvertModelBinder(attribute.Option);
    }
}
