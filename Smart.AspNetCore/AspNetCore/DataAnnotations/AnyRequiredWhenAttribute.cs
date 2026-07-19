namespace Smart.AspNetCore.DataAnnotations;

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AnyRequiredWhenAttribute : ConditionalValidationAttribute
{
    public string[] Properties { get; }

    public bool AllowEmptyStrings { get; set; }

    public AnyRequiredWhenAttribute(string conditionProperty, params string[] properties)
        : base(conditionProperty, false)
    {
        Properties = properties;
    }

    protected override ValidationResult? IsValidValue(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        var modelMetadataProvider = validationContext.GetRequiredService<IModelMetadataProvider>();
        for (var i = 0; i < Properties.Length; i++)
        {
            var metadata = ResolveMetadata(modelMetadataProvider, validationContext.ObjectType, Properties[i]);
            if (HasValue(metadata.PropertyGetter!(validationContext.ObjectInstance)))
            {
                return ValidationResult.Success;
            }
        }

        return new ValidationResult(
            String.Format(CultureInfo.InvariantCulture, ErrorMessage!, ResolveDisplayNames(modelMetadataProvider, validationContext.ObjectType)),
            Properties);
    }

    private static ModelMetadata ResolveMetadata(IModelMetadataProvider modelMetadataProvider, Type objectType, string propertyName)
    {
        var metadata = modelMetadataProvider.GetMetadataForProperty(objectType, propertyName);
        if (metadata is null)
        {
            throw new ArgumentException($"Property {propertyName} is not exist");
        }

        return metadata;
    }

    private object?[] ResolveDisplayNames(IModelMetadataProvider modelMetadataProvider, Type objectType)
    {
        var displayNames = new object?[Properties.Length];
        for (var i = 0; i < Properties.Length; i++)
        {
            displayNames[i] = ResolveMetadata(modelMetadataProvider, objectType, Properties[i]).DisplayName;
        }

        return displayNames;
    }

    private bool HasValue(object? value)
    {
        if (value is null)
        {
            return false;
        }

        return AllowEmptyStrings || value is not string stringValue || !String.IsNullOrWhiteSpace(stringValue);
    }
}
