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
        messageFormat: "Method must have one supported string collection parameter. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor UnconvertibleProperty { get; } = new(
        id: "SAN0003",
        title: "Unconvertible property is not bound",
        messageFormat: "Property has no available converter and is silently skipped. property=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NotPartialContainingType { get; } = new(
        id: "SAN0004",
        title: "Containing type must be partial",
        messageFormat: "Type containing a bind method must be declared partial. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NestedContainingType { get; } = new(
        id: "SAN0005",
        title: "Containing type must be a top-level type",
        messageFormat: "Type containing a bind method must not be nested. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor AbstractTargetType { get; } = new(
        id: "SAN0006",
        title: "Target type must not be abstract",
        messageFormat: "Bind method that creates the target instance requires a non-abstract type. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NoParameterlessConstructor { get; } = new(
        id: "SAN0007",
        title: "Target type requires a parameterless constructor",
        messageFormat: "Bind method that creates the target instance requires an accessible parameterless constructor. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor GenericMethod { get; } = new(
        id: "SAN0008",
        title: "Bind method must not be generic",
        messageFormat: "Bind method must not have type parameters. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
