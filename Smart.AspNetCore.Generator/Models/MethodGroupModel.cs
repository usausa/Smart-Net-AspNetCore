namespace Smart.AspNetCore.Generator.Models;

using SourceGenerateHelper;

internal sealed record MethodGroupModel(
    string Namespace,
    string ClassName,
    EquatableArray<MethodModel> Methods);
