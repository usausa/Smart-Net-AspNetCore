namespace Smart.AspNetCore.Generator.Models;

internal sealed record PropertyModel(
    string Name,
    string TypeName,
    string AssignmentTypeName,
    PropertyValueKind ValueKind,
    bool IsEnum,
    string ConverterMethodTypeName,
    string? ConverterMethodName);
