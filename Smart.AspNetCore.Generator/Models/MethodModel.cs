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
    EquatableArray<PropertyModel> Properties);
