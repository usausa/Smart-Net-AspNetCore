namespace Smart.AspNetCore.DataAnnotations;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class Ms932LengthAttribute : ValidationAttribute
{
    private static readonly Encoding Ms932Encoding;

#pragma warning disable CA1810
    static Ms932LengthAttribute()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Ms932Encoding = Encoding.GetEncoding(932);
    }
#pragma warning restore CA1810

    public int MinimumLength { get; }

    public int MaximumLength { get; }

    public Ms932LengthAttribute(int maximumLength)
    {
        MinimumLength = 0;
        MaximumLength = maximumLength;
        ErrorMessage = "{0} must be at most {1} bytes in MS932 encoding.";
    }

    public Ms932LengthAttribute(int minimumLength, int maximumLength)
    {
        MinimumLength = minimumLength;
        MaximumLength = maximumLength;
        ErrorMessage = "{0} must be between {2} and {1} bytes in MS932 encoding.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str)
        {
            var length = Ms932Encoding.GetByteCount(str);
            if ((length < MinimumLength) || (length > MaximumLength))
            {
                return new ValidationResult(
                    String.Format(CultureInfo.InvariantCulture, ErrorMessage!, validationContext.DisplayName, MaximumLength, MinimumLength),
                    validationContext.MemberName != null ? [validationContext.MemberName] : null);
            }
        }
        else if (value is IEnumerable<string?> ie)
        {
            foreach (var element in ie)
            {
                if (element is not null)
                {
                    var length = Ms932Encoding.GetByteCount(element);
                    if ((length < MinimumLength) || (length > MaximumLength))
                    {
                        return new ValidationResult(
                            String.Format(CultureInfo.InvariantCulture, ErrorMessage!, validationContext.DisplayName, MaximumLength, MinimumLength),
                            validationContext.MemberName != null ? [validationContext.MemberName] : null);
                    }
                }
            }
        }

        return ValidationResult.Success;
    }
}
