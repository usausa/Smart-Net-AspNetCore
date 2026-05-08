namespace Smart.AspNetCore.DataAnnotations;

using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class ElementRequiredAttribute : ValidationAttribute
{
    public bool AllowEmptyStrings { get; set; }

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return false;
        }

        if (value is IEnumerable ie)
        {
            var count = 0;
            foreach (var element in ie)
            {
                if ((element is null) ||
                    (!AllowEmptyStrings && element is string str && String.IsNullOrWhiteSpace(str)))
                {
                    return false;
                }

                count++;
            }

            if (count == 0)
            {
                return false;
            }
        }

        return true;
    }
}
