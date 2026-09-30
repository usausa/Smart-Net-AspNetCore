namespace Smart.AspNetCore.Generator.Models;

using SourceGenerateHelper;

internal sealed record MethodGroupModel(
    string HintName,
    EquatableArray<MethodModel> Methods);
