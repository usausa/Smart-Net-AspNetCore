namespace Smart.AspNetCore.DataAnnotations;

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Property)]
public sealed class CompareToAttribute : ValidationAttribute
{
    private ModelMetadata? otherMetadata;

    public CompareToOperation Operation { get; }

    public string OtherProperty { get; }

    public CompareToAttribute(CompareToOperation operation, string otherProperty)
    {
        Operation = operation;
        OtherProperty = otherProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ResolveMetadata(validationContext);

        var otherValue = otherMetadata.PropertyGetter!(validationContext.ObjectInstance);
        if ((value is not IComparable comparable) || (otherValue is null))
        {
            return ValidationResult.Success;
        }

        var compare = comparable.CompareTo(otherValue);
        if (Operation.IsValidCompare(compare))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            String.Format(CultureInfo.InvariantCulture, ErrorMessage!, validationContext.DisplayName, otherMetadata.DisplayName),
            validationContext.MemberName != null ? [validationContext.MemberName] : null);
    }

    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(otherMetadata))]
    private void ResolveMetadata(ValidationContext validationContext)
    {
        if (otherMetadata is not null)
        {
            return;
        }

        var modelMetadataProvider = validationContext.GetRequiredService<IModelMetadataProvider>();
        otherMetadata = modelMetadataProvider.GetMetadataForProperty(validationContext.ObjectType, OtherProperty);

        if (otherMetadata is null)
        {
            throw new ArgumentException($"Property {OtherProperty} is not exist");
        }
    }
}
