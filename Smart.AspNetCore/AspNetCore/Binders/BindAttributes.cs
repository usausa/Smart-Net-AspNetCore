namespace Smart.AspNetCore.Binders;

using System;

[AttributeUsage(AttributeTargets.Method)]
public sealed class BindAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property)]
public sealed class BindConverterTypeAttribute : Attribute
{
    public BindConverterTypeAttribute(Type converterType)
    {
        ConverterType = converterType;
    }

    public Type ConverterType { get; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class BindIgnoreAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public sealed class BindIgnoreMembersAttribute : Attribute
{
    public BindIgnoreMembersAttribute(params string[] memberNames)
    {
        MemberNames = memberNames;
    }

    public string[] MemberNames { get; }
}
