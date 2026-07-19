namespace Smart.AspNetCore.Generator;

using System.Collections.Immutable;
using System.IO;
using System.Runtime.Loader;

using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Smart.AspNetCore.Binders;

internal static class CompilationHelper
{
    private const string GlobalUsings =
        """
        global using System;
        global using System.Collections.Generic;
        global using System.Linq;
        global using System.Threading;
        global using System.Threading.Tasks;
        """;

    public static void AssertNoGeneratorErrors(GeneratorResult result)
    {
        var errors = result.Diagnostics
            .Where(static x => x.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.True(errors.Length == 0, String.Join(Environment.NewLine, errors.Select(static x => x.ToString())));
    }

    public static GeneratorResult RunGenerator(string source)
    {
        EnsureGeneratorDependenciesLoaded();

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var globalUsings = CSharpSyntaxTree.ParseText(GlobalUsings, parseOptions);

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree, globalUsings],
            GetMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new BindMethodGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        var runResult = driver.GetRunResult();
        var diagnostics = outputCompilation.GetDiagnostics()
            .Concat(generatorDiagnostics)
            .Concat(runResult.Diagnostics)
            .Distinct()
            .ToImmutableArray();
        var sources = runResult.Results
            .SelectMany(static x => x.GeneratedSources)
            .ToDictionary(static x => x.HintName, static x => x.SourceText.ToString());
        var generatedCode = String.Join(
            Environment.NewLine + Environment.NewLine,
            runResult.Results
                .SelectMany(static x => x.GeneratedSources)
                .Select(static x => x.SourceText.ToString()));

        return new GeneratorResult(diagnostics, sources, generatedCode);
    }

    // The generator is referenced as a plain assembly here, so its private dependency must be loaded explicitly.
    private static void EnsureGeneratorDependenciesLoaded()
    {
        var generatorDir = Path.GetDirectoryName(typeof(BindMethodGenerator).Assembly.Location)!;
        var helperDll = Path.Combine(generatorDir, "SourceGenerateHelper.dll");
        if (File.Exists(helperDll))
        {
            AssemblyLoadContext.Default.LoadFromAssemblyPath(helperDll);
        }
    }

    private static ImmutableArray<MetadataReference> GetMetadataReferences()
    {
        var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))?
            .Split(Path.PathSeparator)
            ?? [];
        var assemblyPaths = new HashSet<string>(trustedAssemblies, StringComparer.OrdinalIgnoreCase)
        {
            typeof(BindAttribute).Assembly.Location,
            typeof(IQueryCollection).Assembly.Location
        };

        return [.. assemblyPaths.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))];
    }

    // ReSharper disable once NotAccessedPositionalProperty.Global
    public sealed record GeneratorResult(
        ImmutableArray<Diagnostic> Diagnostics,
        IReadOnlyDictionary<string, string> Sources,
        string GeneratedCode);
}
