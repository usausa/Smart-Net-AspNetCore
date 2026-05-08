namespace Smart.AspNetCore.DataAnnotations;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.InteropServices;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public abstract class DuplicateAttribute : ValidationAttribute
{
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
public abstract class Duplicate2Attribute : ValidationAttribute
{
}

public sealed class DuplicateAttribute<T, TKey> : DuplicateAttribute
    where TKey : notnull
{
    private ModelMetadata? memberMetadata;

    public string Member { get; }

    public DuplicateAttribute(string member)
    {
        Member = member;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IEnumerable<T> ie)
        {
            memberMetadata = ResolveMemberMetadata(validationContext);

            var countsBy = new Dictionary<TKey, int>();
            foreach (var item in ie)
            {
                if (item is null)
                {
                    continue;
                }

                var propertyValue = memberMetadata.PropertyGetter!(item);
                if (propertyValue is TKey key)
                {
                    ref var currentCount = ref CollectionsMarshal.GetValueRefOrAddDefault(countsBy, key, out _);
                    checked
                    {
                        currentCount++;
                    }

                    if (currentCount > 1)
                    {
                        return new ValidationResult(
                            String.Format(CultureInfo.InvariantCulture, ErrorMessage!, memberMetadata.DisplayName),
                            validationContext.MemberName != null ? [validationContext.MemberName] : null);
                    }
                }
                else if (propertyValue is IEnumerable<TKey> keys)
                {
                    foreach (var element in keys)
                    {
                        ref var currentCount = ref CollectionsMarshal.GetValueRefOrAddDefault(countsBy, element, out _);
                        checked
                        {
                            currentCount++;
                        }

                        if (currentCount > 1)
                        {
                            return new ValidationResult(
                                String.Format(CultureInfo.InvariantCulture, ErrorMessage!, memberMetadata.DisplayName),
                                validationContext.MemberName != null ? [validationContext.MemberName] : null);
                        }
                    }
                }
            }
        }

        return ValidationResult.Success;
    }

    private ModelMetadata ResolveMemberMetadata(ValidationContext validationContext) =>
        validationContext.GetRequiredService<IModelMetadataProvider>().GetMetadataForProperty(typeof(T), Member);
}

public sealed class DuplicateAttribute<T, TKey1, TKey2> : Duplicate2Attribute
    where TKey1 : notnull
    where TKey2 : notnull
{
    private ModelMetadata? memberMetadata1;

    private ModelMetadata? memberMetadata2;

    public string Member1 { get; }

    public string Member2 { get; }

    public DuplicateAttribute(string member1, string member2)
    {
        Member1 = member1;
        Member2 = member2;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IEnumerable<T> ie)
        {
            memberMetadata1 = ResolveMemberMetadata1(validationContext);
            memberMetadata2 = ResolveMemberMetadata2(validationContext);

            var countsBy = new Dictionary<(TKey1, TKey2), int>();
            foreach (var item in ie)
            {
                if (item is null)
                {
                    continue;
                }

                var propertyValue1 = memberMetadata1.PropertyGetter!(item);
                var propertyValue2 = memberMetadata2.PropertyGetter!(item);
                if ((propertyValue1 is TKey1 key1) && (propertyValue2 is TKey2 key2))
                {
                    ref var currentCount = ref CollectionsMarshal.GetValueRefOrAddDefault(countsBy, (key1, key2), out _);
                    checked
                    {
                        currentCount++;
                    }

                    if (currentCount > 1)
                    {
                        return new ValidationResult(
                            String.Format(CultureInfo.InvariantCulture, ErrorMessage!, memberMetadata1.DisplayName, memberMetadata2.DisplayName),
                            validationContext.MemberName != null ? [validationContext.MemberName] : null);
                    }
                }
            }
        }

        return ValidationResult.Success;
    }

    private ModelMetadata ResolveMemberMetadata1(ValidationContext validationContext) =>
        validationContext.GetRequiredService<IModelMetadataProvider>().GetMetadataForProperty(typeof(T), Member1);

    private ModelMetadata ResolveMemberMetadata2(ValidationContext validationContext) =>
        validationContext.GetRequiredService<IModelMetadataProvider>().GetMetadataForProperty(typeof(T), Member2);
}
