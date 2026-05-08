namespace Smart.AspNetCore.Providers;

using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

public sealed class RemoveBodyRequiredProvider : IValidationMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        if (context.Attributes.OfType<FromBodyAttribute>().Any())
        {
            var required = context.ValidationMetadata.ValidatorMetadata.OfType<RequiredAttribute>().FirstOrDefault();
            if (required is not null)
            {
                context.ValidationMetadata.ValidatorMetadata.Remove(required);
            }
        }
    }
}
