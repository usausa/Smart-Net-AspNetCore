namespace Smart.AspNetCore.Generator;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidMethodDefinition { get; } = new(
        id: "SAN0001",
        title: "Invalid method definition",
        messageFormat: "[Bind] method must be static partial without an implementation. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor InvalidMethodParameter { get; } = new(
        id: "SAN0002",
        title: "Invalid method parameter",
        messageFormat: "[Bind] method must take one string collection. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor UnconvertibleProperty { get; } = new(
        id: "SAN0003",
        title: "Unconvertible property is not bound",
        messageFormat: "Property has no available converter. property=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NotPartialContainingType { get; } = new(
        id: "SAN0004",
        title: "Containing type must be partial",
        messageFormat: "[Bind] containing type is not partial. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor NestedContainingType { get; } = new(
        id: "SAN0005",
        title: "Containing type must be a top-level type",
        messageFormat: "[Bind] containing type must not be nested or file-local. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor AbstractTargetType { get; } = new(
        id: "SAN0006",
        title: "Target type must not be abstract",
        messageFormat: "Target type is abstract. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor NoParameterlessConstructor { get; } = new(
        id: "SAN0007",
        title: "Parameterless constructor required",
        messageFormat: "Target type cannot be created with a parameterless constructor (or has required members). type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor GenericMethod { get; } = new(
        id: "SAN0008",
        title: "Bind method must not be generic",
        messageFormat: "[Bind] method must not be generic. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);

    public static DiagnosticDescriptor HintNameCollision { get; } = new(
        id: "SAN0009",
        title: "Type name differs only in case",
        messageFormat: "Type name differs only in case from another type, and its source is not generated. type=[{0}], other=[{1}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: DiagnosticTags.NotSuppressible);
}
