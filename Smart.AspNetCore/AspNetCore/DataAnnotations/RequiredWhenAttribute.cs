namespace Smart.AspNetCore.DataAnnotations;

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredWhenAttribute : ConditionalValidationAttribute
{
    public bool AllowEmptyStrings { get; set; }

    public RequiredWhenAttribute(string conditionProperty)
        : base(conditionProperty, true)
    {
    }

    protected override ValidationResult? IsValidValue(object? value, ValidationContext validationContext)
    {
        if ((value is not null) &&
            (AllowEmptyStrings || value is not string stringValue || !String.IsNullOrWhiteSpace(stringValue)))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            String.Format(CultureInfo.InvariantCulture, ErrorMessage!, validationContext.DisplayName),
            validationContext.MemberName != null ? [validationContext.MemberName] : null);
    }
}
