namespace Smart.AspNetCore.Generator;

using System.Reflection;

using Microsoft.CodeAnalysis;

public class BindingCodeTests
{
    private const string Head =
        """
        #nullable enable
        using Microsoft.AspNetCore.Http;
        using Smart.AspNetCore.Binders;

        """;

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    [Fact]
    public void BaseClassPropertiesAreBound()
    {
        const string source = Head + """
            internal abstract class PagingRequest
            {
                public int Page { get; set; }

                public int PageSize { get; set; }
            }

            internal sealed class SearchRequest : PagingRequest
            {
                public string? Keyword { get; set; }
            }

            internal static partial class Binder
            {
                [Bind]
                public static partial SearchRequest BindSearch(IQueryCollection query);
            }
            """;

        var result = CompilationHelper.RunGenerator(source);

        Assert.Empty(CompilationHelper.GetProblemIds(source));
        Assert.Contains("target.Page = ", result.GeneratedCode, StringComparison.Ordinal);
        Assert.Contains("target.PageSize = ", result.GeneratedCode, StringComparison.Ordinal);
        Assert.Contains("target.Keyword = ", result.GeneratedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void UnassignablePropertiesAreNotBound()
    {
        const string source = Head + """
            internal sealed class SearchRequest
            {
                public int Id { get; set; }

                public int InitOnly { get; init; }

                public int PrivateSet { get; private set; }

                public int this[int index] { get => 0; set { } }
            }

            internal static partial class Binder
            {
                [Bind]
                public static partial void BindSearch(IQueryCollection query, SearchRequest target);
            }
            """;

        var result = CompilationHelper.RunGenerator(source);

        Assert.Empty(CompilationHelper.GetProblemIds(source));
        Assert.Contains("target.Id = ", result.GeneratedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("InitOnly", result.GeneratedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("PrivateSet", result.GeneratedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void San0007RequiredMemberWithFactoryEmitsDiagnostic()
    {
        const string source = Head + """
            internal sealed class SearchRequest
            {
                public required string Keyword { get; set; }
            }

            internal static partial class Binder
            {
                [Bind]
                public static partial SearchRequest BindSearch(IQueryCollection query);
            }
            """;

        Assert.Equal(["SAN0007"], CompilationHelper.GetProblemIds(source));
    }

    [Fact]
    public void StringPropertiesCompileWithoutWarnings()
    {
        const string source = Head + """
            internal sealed class SearchRequest
            {
                public string Keyword { get; set; } = string.Empty;

                public string[] Tags { get; set; } = [];

                public int[] Ids { get; set; } = [];
            }

            internal static partial class Binder
            {
                [Bind]
                public static partial SearchRequest BindSearch(IQueryCollection query);
            }
            """;

        Assert.Empty(CompilationHelper.GetProblemIds(source));
    }

    // ------------------------------------------------------------
    // Declaration
    // ------------------------------------------------------------

    [Theory]
    [InlineData("static partial void BindSearch(IQueryCollection query, SearchRequest request);")]
    [InlineData("public static partial void BindSearch(IQueryCollection query, SearchRequest request);")]
    [InlineData("public static partial SearchRequest BindSearch(this IQueryCollection query, SearchRequest request);")]
    [InlineData("public static partial SearchRequest BindSearch(IQueryCollection target);")]
    [InlineData("public static partial SearchRequest @event(IQueryCollection @in);")]
    public void ImplementationRepeatsDeclaration(string method)
    {
        var source = Head + $$"""
            internal sealed class SearchRequest
            {
                public int Page { get; set; }
            }

            internal static partial class Binder
            {
                [Bind]
                {{method}}
            }
            """;

        Assert.Empty(CompilationHelper.GetProblemIds(source));
    }

    // An implementation that throws replaces the one not generated, unless an outer type cannot take another part
    [Theory]
    [InlineData("internal static partial class Outer", new[] { "SAN0005" })]
    [InlineData("internal static class Outer", new[] { "SAN0005", "CS8795" })]
    public void ErrorGeneratesThrowingImplementationWhenPossible(string outer, string[] expected)
    {
        var source = Head + $$"""
            internal sealed class SearchRequest
            {
                public int Page { get; set; }
            }

            {{outer}}
            {
                internal static partial class Binder
                {
                    [Bind]
                    public static partial SearchRequest BindSearch(IQueryCollection query);
                }
            }
            """;

        Assert.Equal(expected, CompilationHelper.GetProblemIds(source));
    }

    // ------------------------------------------------------------
    // Diagnostics
    // ------------------------------------------------------------

    [Fact]
    public void WarningCanBeSuppressedAtProperty()
    {
        const string source = Head + """
            internal sealed class SearchRequest
            {
            #pragma warning disable SAN0003
                public System.Text.StringBuilder Builder { get; set; } = new();
            #pragma warning restore SAN0003
            }

            internal static partial class Binder
            {
                [Bind]
                public static partial SearchRequest BindSearch(IQueryCollection query);
            }
            """;

        Assert.DoesNotContain("SAN0003", CompilationHelper.GetProblemIds(source));
    }

    [Fact]
    public void ErrorsCannotBeSuppressed()
    {
        var descriptors = typeof(Diagnostics)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(static x => x.PropertyType == typeof(DiagnosticDescriptor))
            .Select(static x => (DiagnosticDescriptor)x.GetValue(null)!)
            .ToList();

        Assert.All(
            descriptors.Where(static x => x.DefaultSeverity == DiagnosticSeverity.Error),
            static x => Assert.Equal([WellKnownDiagnosticTags.NotConfigurable, WellKnownDiagnosticTags.Compiler], x.CustomTags));
        Assert.All(
            descriptors.Where(static x => x.DefaultSeverity != DiagnosticSeverity.Error),
            static x => Assert.Empty(x.CustomTags));
    }

    // ------------------------------------------------------------
    // Converters returning null
    // ------------------------------------------------------------

    [Fact]
    public void NullConverterResultIsNotAssigned()
    {
        const string source = Head + """
            internal static class CountConverter
            {
                public static int? ToInt32(System.ReadOnlySpan<char> value) =>
                    int.TryParse(value, out var result) ? result : null;
            }

            [BindConverter(typeof(CountConverter))]
            internal sealed class SearchRequest
            {
                public int Count { get; set; } = 10;

                public int? Limit { get; set; } = 20;

                public int[] Values { get; set; } = [];
            }

            internal static partial class Binder
            {
                [Bind]
                public static partial SearchRequest BindSearch(IQueryCollection query);
            }
            """;

        var result = CompilationHelper.RunGenerator(source);

        Assert.Empty(CompilationHelper.GetProblemIds(source));
        Assert.Contains("global::CountConverter.ToInt32(", result.GeneratedCode, StringComparison.Ordinal);
        Assert.Contains("is { } __p0v)", result.GeneratedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("DefaultStringConverter.ToInt32(", result.GeneratedCode, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------
    // Nullable targets
    // ------------------------------------------------------------

    [Fact]
    public void NullableTargetIsLeftWhenNull()
    {
        const string source = Head + """
            internal sealed class SearchRequest
            {
                public int Page { get; set; }
            }

            internal static partial class Binder
            {
                [Bind]
                public static partial void Bind(IQueryCollection query, SearchRequest? target);

                [Bind]
                public static partial SearchRequest? BindReturn(IQueryCollection query, SearchRequest? target);
            }
            """;

        var result = CompilationHelper.RunGenerator(source);

        Assert.Empty(CompilationHelper.GetProblemIds(source));
        Assert.Contains("if (target is null)", result.GeneratedCode, StringComparison.Ordinal);
        Assert.Contains("return target!;", result.GeneratedCode, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------
    // Generated names
    // ------------------------------------------------------------

    [Fact]
    public void GeneratedLocalsDoNotClashWithParameterNames()
    {
        const string source = Head + """
            internal sealed class SearchRequest
            {
                public int Page { get; set; }

                public int[] Ids { get; set; } = [];
            }

            internal static partial class Binder
            {
                [Bind]
                public static partial SearchRequest Bind(IQueryCollection p0);

                [Bind]
                public static partial SearchRequest BindTarget(IQueryCollection target);

                [Bind]
                public static partial void BindInto(IQueryCollection i, SearchRequest p1arr);
            }
            """;

        Assert.Empty(CompilationHelper.GetProblemIds(source));
    }

    [Fact]
    public void ObsoletePropertiesCompileAndErrorOnesAreNotBound()
    {
        const string source = Head + """
            internal sealed class SearchRequest
            {
                [System.Obsolete]
                public int Page { get; set; }

                [System.Obsolete("old", true)]
                public int Old { get; set; }
            }

            internal static partial class Binder
            {
                [Bind]
                public static partial SearchRequest Bind(IQueryCollection query);
            }
            """;

        var result = CompilationHelper.RunGenerator(source);

        Assert.Empty(CompilationHelper.GetProblemIds(source));
        Assert.Contains("__target.Page = ", result.GeneratedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("__target.Old = ", result.GeneratedCode, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------
    // SAN0009 : type names differing only in case
    // ------------------------------------------------------------

    [Fact]
    public void San0009CaseOnlyTypeNamesGenerateTheFirstOnly()
    {
        const string source = Head + """
            internal sealed class SearchRequest
            {
                public int Page { get; set; }
            }

            internal static partial class Binder
            {
                [Bind]
                public static partial SearchRequest Bind(IQueryCollection query);
            }

            internal static partial class binder
            {
                [Bind]
                public static partial SearchRequest Bind(IQueryCollection query);
            }
            """;

        var problems = CompilationHelper.GetProblemIds(source);

        Assert.Contains("SAN0009", problems);
        Assert.DoesNotContain("CS8785", problems);
    }
}
