namespace Smart.AspNetCore.Generator.Models;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

internal sealed record MethodModel(
    // Containing type
    string Namespace,
    string ClassName,
    bool IsStatic,
    bool IsValueType,
    // Method signature
    Accessibility MethodAccessibility,
    string MethodName,
    string ReturnTypeName,
    // Binding target and source
    BindingPattern Pattern,
    string TargetTypeName,
    string SourceTypeName,
    string SourceValueKind,
    string SourceParameterName,
    EquatableArray<PropertyModel> Properties,
    // Options
    bool IsExtensionMethod,
    bool Strict,
    // Diagnostics
    EquatableArray<DiagnosticInfo> Diagnostics);
