namespace Smart.AspNetCore.Generator.Models;

internal sealed record PropertyModel(
    string Name,
    string TypeName,
    string AssignmentTypeName,
    bool IsString,
    bool IsStringArray,
    bool IsArray,
    bool IsEnum,
    bool IsIgnored,
    bool HasSetter,
    string ConverterMethodTypeName,
    string? ConverterMethodName);
