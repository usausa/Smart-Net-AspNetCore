namespace Smart.AspNetCore.Generator;

using System.Globalization;

public sealed class BindMethodGeneratorDiagnosticTest
{
    private const string Head =
        """
        using Microsoft.AspNetCore.Http;
        using Smart.AspNetCore.Binders;


        """;

    [Fact]
    public void UnconvertiblePropertyReportsSan0003()
    {
        var result = CompilationHelper.RunGenerator(Head + """
            internal sealed class SampleTarget
            {
                public int Id { get; set; }

                public System.Text.StringBuilder Builder { get; set; } = new();
            }

            internal static partial class SampleBinder
            {
                [Bind]
                public static partial SampleTarget BindSample(IQueryCollection query);
            }
            """);

        var reported = Assert.Single(result.Diagnostics, static x => x.Id == "SAN0003");
        Assert.Contains("Builder", reported.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    [Fact]
    public void NonPartialContainingTypeReportsSan0004()
    {
        const string source = Head + """
            internal sealed class Target { public int Id { get; set; } }

            internal static class Binder
            {
                [Bind]
                public static partial Target Bind(IQueryCollection query);
            }
            """;

        AssertReported("SAN0004", source);
    }

    [Fact]
    public void NestedContainingTypeReportsSan0005()
    {
        const string source = Head + """
            internal sealed class Target { public int Id { get; set; } }

            internal static class Outer
            {
                internal static partial class Binder
                {
                    [Bind]
                    public static partial Target Bind(IQueryCollection query);
                }
            }
            """;

        AssertReported("SAN0005", source);
    }

    [Fact]
    public void AbstractTargetReportsSan0006()
    {
        const string source = Head + """
            internal abstract class Target { public int Id { get; set; } }

            internal static partial class Binder
            {
                [Bind]
                public static partial Target Bind(IQueryCollection query);
            }
            """;

        AssertReported("SAN0006", source);
    }

    [Fact]
    public void TargetWithoutParameterlessConstructorReportsSan0007()
    {
        const string source = Head + """
            internal sealed class Target
            {
                public Target(int x) { Id = x; }

                public int Id { get; set; }
            }

            internal static partial class Binder
            {
                [Bind]
                public static partial Target Bind(IQueryCollection query);
            }
            """;

        AssertReported("SAN0007", source);
    }

    [Fact]
    public void GenericMethodReportsSan0008()
    {
        const string source = Head + """
            internal sealed class Target { public int Id { get; set; } }

            internal static partial class Binder
            {
                [Bind]
                public static partial Target Bind<TX>(IQueryCollection query);
            }
            """;

        AssertReported("SAN0008", source);
    }

    [Fact]
    public void GenericContainingTypeIsSupported()
    {
        var result = CompilationHelper.RunGenerator(Head + """
            internal sealed class Target { public int Id { get; set; } }

            internal static partial class Binder<TX>
            {
                [Bind]
                public static partial Target Bind(IQueryCollection query);
            }
            """);

        CompilationHelper.AssertNoGeneratorErrors(result);
        Assert.DoesNotContain(result.Diagnostics, static x => x.Id.StartsWith("SAN", StringComparison.Ordinal));
    }

    [Fact]
    public void OverloadedBindMethodsAreSupported()
    {
        var result = CompilationHelper.RunGenerator(Head + """
            internal sealed class Target { public int Id { get; set; } }

            internal static partial class Binder
            {
                [Bind]
                public static partial Target Bind(IQueryCollection query);

                [Bind]
                public static partial Target Bind(IFormCollection form);
            }
            """);

        CompilationHelper.AssertNoGeneratorErrors(result);
        Assert.DoesNotContain(result.Diagnostics, static x => x.Id.StartsWith("SAN", StringComparison.Ordinal));
    }

    [Fact]
    public void ConvertiblePropertiesReportNoDiagnostic()
    {
        var result = CompilationHelper.RunGenerator(Head + """
            internal sealed class ConvertibleTarget
            {
                public int Id { get; set; }

                public string? Name { get; set; }
            }

            internal static partial class ConvertibleBinder
            {
                [Bind]
                public static partial ConvertibleTarget BindConvertible(IQueryCollection query);
            }
            """);

        CompilationHelper.AssertNoGeneratorErrors(result);
        Assert.DoesNotContain(result.Diagnostics, static x => x.Id == "SAN0003");
        Assert.Contains("ToInt32", result.GeneratedCode, StringComparison.Ordinal);
    }

    private static void AssertReported(string expectedId, string source)
    {
        var result = CompilationHelper.RunGenerator(source);

        Assert.Contains(result.Diagnostics, x => x.Id == expectedId);
    }
}
