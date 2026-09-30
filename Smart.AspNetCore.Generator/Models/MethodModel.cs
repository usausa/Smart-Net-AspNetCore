namespace Smart.AspNetCore.Generator.Models;

using SourceGenerateHelper;

internal sealed record MethodModel(
    // Containing type
    string Namespace,
    EquatableArray<string> ContainingTypes,
    string HintName,
    // Method signature
    string Signature,
    // Binding target and source
    BindingPattern Pattern,
    string TargetTypeName,
    string TargetName,
    string SourceValueKind,
    string SourceParameterName,
    EquatableArray<PropertyModel> Properties,
    // Options
    bool Strict,
    // Diagnostics
    EquatableArray<DiagnosticInfo> Diagnostics,
    bool IsFallback = false,
    bool TargetNullable = false,
    string TypeName = "");
