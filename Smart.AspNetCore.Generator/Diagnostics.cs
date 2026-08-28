namespace Smart.AspNetCore.Generator;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidMethodDefinition { get; } = new(
        id: "SAN0001",
        title: "Invalid method definition",
        messageFormat: "Method must be static partial. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidMethodParameter { get; } = new(
        id: "SAN0002",
        title: "Invalid method parameter",
        messageFormat: "Method must take one string collection. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

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
        messageFormat: "Containing type is not partial. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NestedContainingType { get; } = new(
        id: "SAN0005",
        title: "Containing type must be a top-level type",
        messageFormat: "Containing type must not be nested. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor AbstractTargetType { get; } = new(
        id: "SAN0006",
        title: "Target type must not be abstract",
        messageFormat: "Target type is abstract. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NoParameterlessConstructor { get; } = new(
        id: "SAN0007",
        title: "Parameterless constructor required",
        messageFormat: "Target type has no parameterless constructor. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor GenericMethod { get; } = new(
        id: "SAN0008",
        title: "Bind method must not be generic",
        messageFormat: "Method must not be generic. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
