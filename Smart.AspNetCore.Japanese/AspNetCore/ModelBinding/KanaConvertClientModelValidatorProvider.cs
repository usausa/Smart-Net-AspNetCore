namespace Smart.AspNetCore.ModelBinding;

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

internal sealed class KanaConvertClientModelValidatorProvider : IClientModelValidatorProvider
{
    public void CreateValidators(ClientValidatorProviderContext context)
    {
        var attribute = context.ModelMetadata is DefaultModelMetadata meta
            ? meta.Attributes.PropertyAttributes?.OfType<KanaConvertAttribute>().FirstOrDefault()
            : null;
        if ((attribute is null) || attribute.ClientValidation)
        {
            return;
        }

        foreach (var item in context.Results)
        {
            if (item.ValidatorMetadata is not RequiredAttribute)
            {
                item.Validator = null;
            }
        }
    }
}
