namespace Smart.AspNetCore.DataAnnotations;

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

public abstract class ConditionalValidationAttribute : ValidationAttribute
{
    private readonly bool allowNull;

    private ModelMetadata? conditionMetadata;

    public string ConditionProperty { get; }

    protected ConditionalValidationAttribute(string conditionProperty, bool allowNull)
    {
        ConditionProperty = conditionProperty;
        this.allowNull = allowNull;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (String.IsNullOrEmpty(ConditionProperty))
        {
            return IsValidValue(value, validationContext);
        }

        if (!allowNull && (value is null))
        {
            return ValidationResult.Success;
        }

        ResolveMetadata(validationContext);

        var condition = conditionMetadata.PropertyGetter!(validationContext.ObjectInstance);
        return condition is not true
            ? ValidationResult.Success
            : IsValidValue(value, validationContext);
    }

    protected abstract ValidationResult? IsValidValue(object? value, ValidationContext validationContext);

    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(conditionMetadata))]
    private void ResolveMetadata(ValidationContext validationContext)
    {
        if (conditionMetadata is not null)
        {
            return;
        }

        var modelMetadataProvider = validationContext.GetRequiredService<IModelMetadataProvider>();
        conditionMetadata = modelMetadataProvider.GetMetadataForProperty(validationContext.ObjectType, ConditionProperty);

        if (conditionMetadata is null)
        {
            throw new ArgumentException($"Property {ConditionProperty} is not exist");
        }
    }
}
