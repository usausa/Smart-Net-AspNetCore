namespace Smart.AspNetCore.DataAnnotations;

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

public abstract class ConditionalValidationAttribute : ValidationAttribute
{
    private readonly bool allowNull;

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

        var conditionMetadata = validationContext.GetRequiredService<IModelMetadataProvider>().GetMetadataForProperty(validationContext.ObjectType, ConditionProperty);
        if (conditionMetadata is null)
        {
            throw new ArgumentException($"Property {ConditionProperty} is not exist");
        }

        var condition = conditionMetadata.PropertyGetter!(validationContext.ObjectInstance);
        return condition is not true ? ValidationResult.Success : IsValidValue(value, validationContext);
    }

    protected abstract ValidationResult? IsValidValue(object? value, ValidationContext validationContext);
}
