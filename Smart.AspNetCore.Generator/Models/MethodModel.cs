namespace Smart.AspNetCore.Generator.Models;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

internal sealed record MethodModel(
    string Namespace,
    string ClassName,
    bool IsStatic,
    bool IsValueType,
    Accessibility MethodAccessibility,
    string MethodName,
    string ReturnTypeName,
    BindingPattern Pattern,
    string TargetTypeName,
    string SourceTypeName,
    string SourceValueKind,
    string SourceParameterName,
    bool IsExtensionMethod,
    EquatableArray<PropertyModel> Properties);
