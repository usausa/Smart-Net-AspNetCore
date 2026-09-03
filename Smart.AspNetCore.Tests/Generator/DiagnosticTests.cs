namespace Smart.AspNetCore.Generator;

using System.Globalization;

public class DiagnosticTests
{
    private const string Head =
        """
        using Microsoft.AspNetCore.Http;
        using Smart.AspNetCore.Binders;

        """;

    // ------------------------------------------------------------
    // Method definition
    // ------------------------------------------------------------

    [Fact]
    public void San0001NonStaticMethodEmitsDiagnostic()
    {
        // Arrange
        const string source = Head + """
            internal sealed class Target { public int Id { get; set; } }

            internal partial class Binder
            {
                [Bind]
                public partial Target BindSample(IQueryCollection query);
            }
            """;

        // Act
        var result = CompilationHelper.RunGenerator(source);

        // Assert
        Assert.Contains(result.Diagnostics, static x => x.Id == "SAN0001");
    }

    [Fact]
    public void San0002NoParameterEmitsDiagnostic()
    {
        // Arrange
        const string source = Head + """
            internal sealed class Target { public int Id { get; set; } }

            internal static partial class Binder
            {
                [Bind]
                public static partial Target BindSample();
            }
            """;

        // Act
        var result = CompilationHelper.RunGenerator(source);

        // Assert
        Assert.Contains(result.Diagnostics, static x => x.Id == "SAN0002");
    }

    [Fact]
    public void San0002UnsupportedParameterTypeEmitsDiagnostic()
    {
        // Arrange
        const string source = Head + """
            internal sealed class Target { public int Id { get; set; } }

            internal static partial class Binder
            {
                [Bind]
                public static partial Target BindSample(int query);
            }
            """;

        // Act
        var result = CompilationHelper.RunGenerator(source);

        // Assert
        Assert.Contains(result.Diagnostics, static x => x.Id == "SAN0002");
    }

    // ------------------------------------------------------------
    // Property binding
    // ------------------------------------------------------------
    [Fact]
    public void San0003UnconvertiblePropertyEmitsDiagnostic()
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
    public void San0004NonPartialContainingTypeEmitsDiagnostic()
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
    public void San0005NestedContainingTypeEmitsDiagnostic()
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
    public void San0006AbstractTargetEmitsDiagnostic()
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
    public void San0007TargetWithoutParameterlessConstructorEmitsDiagnostic()
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
    public void San0008GenericMethodEmitsDiagnostic()
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
    public void San0003ConvertiblePropertiesReportEmitsNoDiagnostic()
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
