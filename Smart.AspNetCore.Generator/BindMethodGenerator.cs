namespace Smart.AspNetCore.Generator;

using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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

        context.RegisterSourceOutput(
            methodProvider,
            static (context, methods) => ReportDiagnostics(context, methods));

        var groups = methodProvider.SelectMany(static (methods, _) =>
            methods.SelectValue()
                .GroupBy(static x => (x.Namespace, x.ClassName))
                .Select(static g => new MethodGroupModel(g.Key.Namespace, g.Key.ClassName, new EquatableArray<MethodModel>(g.ToArray())))
                .ToImmutableArray());
        context.RegisterImplementationSourceOutput(
            groups,
            static (context, group) => Execute(context, group));
    }

    private static void ReportDiagnostics(SourceProductionContext context, ImmutableArray<Result<MethodModel>> methods)
    {
        foreach (var info in methods.SelectError())
        {
            context.ReportDiagnostic(info);
        }

        foreach (var model in methods.SelectValue())
        {
            var modelDiagnostics = model.Diagnostics;
            for (var i = 0; i < modelDiagnostics.Count; i++)
            {
                context.ReportDiagnostic(modelDiagnostics[i]);
            }
        }
    }

    private static void Execute(SourceProductionContext context, MethodGroupModel group)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var builder = new SourceBuilder();
        BindMethodSourceBuilder.BuildSource(builder, group.Methods.ToList());
        var filename = BindMethodSourceBuilder.MakeFilename(group.Namespace, group.ClassName);
        context.AddSource(filename, SourceText.From(builder.ToString(), Encoding.UTF8));
    }
}
