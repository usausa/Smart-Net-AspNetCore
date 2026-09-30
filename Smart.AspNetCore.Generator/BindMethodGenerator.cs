namespace Smart.AspNetCore.Generator;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Smart.AspNetCore.Generator.Models;

using SourceGenerateHelper;

[Generator]
public sealed class BindMethodGenerator : IIncrementalGenerator
{
    private const string BindAttributeName = "Smart.AspNetCore.Binders.BindAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                BindAttributeName,
                static (syntax, _) => syntax is MethodDeclarationSyntax,
                static (context, _) => BindMethodModelBuilder.GetMethodModel(context))
            .Collect();

        var treeProvider = context.ForAttributeWithMetadataNameSyntaxTrees(
            BindAttributeName,
            static (syntax, _) => syntax is MethodDeclarationSyntax);

        context.RegisterSourceOutput(
            methodProvider.Combine(treeProvider),
            static (context, provider) => ReportDiagnostics(context, provider.Left, provider.Right));

        var groups = methodProvider.SelectMany(static (methods, _) => SelectGroups(methods));
        context.RegisterImplementationSourceOutput(
            groups,
            static (context, group) => Execute(context, group));
    }

    private static void ReportDiagnostics(SourceProductionContext context, ImmutableArray<Result<MethodModel>> methods, ImmutableArray<SyntaxTree> trees)
    {
        var diagnostics = methods.SelectError()
            .Concat(methods.SelectValue().SelectMany(static x => x.Diagnostics))
            .Concat(FindHintNameCollisions(methods).Values)
            .Distinct();
        context.ReportDiagnostics(diagnostics, trees);
    }

    private static ImmutableArray<MethodGroupModel> SelectGroups(ImmutableArray<Result<MethodModel>> methods)
    {
        var collisions = FindHintNameCollisions(methods);
        return methods.SelectValue()
            .Where(x => !collisions.ContainsKey(x.HintName))
            .GroupBy(static x => x.HintName)
            .Select(static x => new MethodGroupModel(x.Key, new EquatableArray<MethodModel>(x)))
            .ToImmutableArray();
    }

    private static Dictionary<string, DiagnosticInfo> FindHintNameCollisions(ImmutableArray<Result<MethodModel>> methods)
    {
        var collisions = new Dictionary<string, DiagnosticInfo>(StringComparer.Ordinal);
        var firsts = new Dictionary<string, MethodModel>(StringComparer.OrdinalIgnoreCase);
        foreach (var method in methods.SelectValue().OrderBy(static x => x.HintName, StringComparer.Ordinal))
        {
            if (!firsts.TryGetValue(method.HintName, out var first))
            {
                firsts.Add(method.HintName, method);
            }
            else if ((first.HintName != method.HintName) && !collisions.ContainsKey(method.HintName))
            {
                collisions.Add(method.HintName, new DiagnosticInfo(Diagnostics.HintNameCollision, (Location?)null, method.TypeName, first.TypeName));
            }
        }

        return collisions;
    }

    private static void Execute(SourceProductionContext context, MethodGroupModel group)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var builder = new SourceBuilder();
        BindMethodSourceBuilder.BuildSource(builder, group.Methods);
        context.AddSource(group.HintName, builder);
    }
}
