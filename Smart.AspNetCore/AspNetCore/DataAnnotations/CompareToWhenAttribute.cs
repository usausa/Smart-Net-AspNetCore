namespace Smart.AspNetCore.DataAnnotations;

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Property)]
public sealed class CompareToWhenAttribute : ConditionalValidationAttribute
{
    public CompareToOperation Operation { get; }

    public string OtherProperty { get; }

    public CompareToWhenAttribute(string conditionProperty, CompareToOperation operation, string otherProperty)
        : base(conditionProperty, true)
    {
        Operation = operation;
        OtherProperty = otherProperty;
    }

    protected override ValidationResult? IsValidValue(object? value, ValidationContext validationContext)
    {
        if (value is not IComparable comparable)
        {
            return ValidationResult.Success;
        }

        var otherMetadata = validationContext.GetRequiredService<IModelMetadataProvider>().GetMetadataForProperty(validationContext.ObjectType, OtherProperty);
        if (otherMetadata is null)
        {
            throw new ArgumentException($"Property {OtherProperty} is not exist");
        }

        var otherValue = otherMetadata.PropertyGetter!(validationContext.ObjectInstance);
        if (otherValue is null)
        {
            return ValidationResult.Success;
        }

        if (otherValue.GetType() != value.GetType())
        {
            throw new InvalidOperationException($"CompareTo requires the same type. property=[{validationContext.MemberName}]({value.GetType()}), other=[{OtherProperty}]({otherValue.GetType()}).");
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
}
