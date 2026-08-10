namespace Smart.AspNetCore.Generator;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;

using Smart.AspNetCore.Binders;

using SourceGenerateHelper.Testing;

internal static class CompilationHelper
{
    private static GeneratorTestRunner Runner => GeneratorTestRunner
        .For<BindMethodGenerator>()
        .WithReference(typeof(BindAttribute).Assembly)
        .WithReference(typeof(IQueryCollection).Assembly);

    public static GeneratorResult RunGenerator(string source)
    {
        var result = Runner.Run(source);

        return new GeneratorResult(
            [.. result.GeneratorDiagnostics],
            result.GeneratedSources,
            result.AllGeneratedText);
    }

    public static void AssertNoGeneratorErrors(GeneratorResult result)
    {
        var errors = result.Diagnostics
            .Where(static x => x.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.True(errors.Length == 0, String.Join(Environment.NewLine, errors.Select(static x => x.ToString())));
    }

    public sealed record GeneratorResult(
        ImmutableArray<Diagnostic> Diagnostics,
        IReadOnlyDictionary<string, string> Sources,
        string GeneratedCode);
}
