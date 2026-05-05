namespace Smart.AspNetCore.Generator.Models;

using SourceGenerateHelper;

internal sealed record ConverterTypeModel(
    string TypeName,
    EquatableArray<ConverterMethodModel> Methods);
