namespace Smart.AspNetCore.Generator.Models;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

internal sealed record MethodModel(
    string Namespace,
    string ClassName,
    bool IsValueType,
    bool IsStatic,
    Accessibility MethodAccessibility,
    string MethodName,
    string ReturnTypeName,
    bool HasTargetParameter,
    string TargetTypeName,
    string SourceTypeName,
    string SourceValueKind,
    string SourceParameterName,
    string? MethodConverterTypeName,
    string? ContainingTypeConverterTypeName,
    EquatableArray<string> IgnoredMemberNames,
    EquatableArray<PropertyModel> Properties);

internal sealed record PropertyModel(
    string Name,
    string TypeName,
    string AssignmentTypeName,
    string? ConverterTypeName,
    bool IsString,
    bool IsStringArray,
    bool IsArray,
    bool IsEnum,
    bool IsIgnored,
    bool HasSetter,
    string ConverterMethodTypeName,
    string? ConverterMethodName);

internal sealed record ConverterTypeModel(
    string TypeName,
    EquatableArray<ConverterMethodModel> Methods);

internal sealed record ConverterMethodModel(
    string Name,
    string ReturnTypeName);
