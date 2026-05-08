namespace Smart.AspNetCore.DataAnnotations;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AnyRequiredWhenAttribute : ConditionalValidationAttribute
{
    private ModelMetadata[] metadata = [];

    private object?[] displayNames = [];

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

        ResolveMetadata(validationContext);

        if (metadata.Any(md => HasValue(md.PropertyGetter!(validationContext.ObjectInstance))))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(String.Format(CultureInfo.InvariantCulture, ErrorMessage!, displayNames), Properties);
    }

    private void ResolveMetadata(ValidationContext validationContext)
    {
        if (metadata.Length == Properties.Length)
        {
            return;
        }

        var modelMetadataProvider = validationContext.GetRequiredService<IModelMetadataProvider>();
        metadata = new ModelMetadata[Properties.Length];
        displayNames = new object[Properties.Length];

        for (var i = 0; i < Properties.Length; i++)
        {
            var md = modelMetadataProvider.GetMetadataForProperty(validationContext.ObjectType, Properties[i]);
            if (md is null)
            {
                throw new ArgumentException($"Property {Properties[i]} is not exist");
            }

            metadata[i] = md;
            displayNames[i] = md.DisplayName!;
        }
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
