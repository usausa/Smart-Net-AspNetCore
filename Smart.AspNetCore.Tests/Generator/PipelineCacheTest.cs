namespace Smart.AspNetCore.Generator;

using SourceGenerateHelper.Testing;

public sealed class PipelineCacheTest
{
    private const string Source =
        """
        using Microsoft.AspNetCore.Http;
        using Smart.AspNetCore.Binders;

        internal sealed class SampleTarget
        {
            public int Id { get; set; }
        }

        internal static partial class SampleBinder
        {
            [Bind]
            public static partial SampleTarget BindSample(IQueryCollection query);
        }
        """;

    private const string UnrelatedSource =
        """
        namespace Other;

        internal sealed class Unrelated;
        """;

    private const string AddedTargetSource =
        """
        using Microsoft.AspNetCore.Http;
        using Smart.AspNetCore.Binders;

        internal sealed class AddedTarget
        {
            public int Id { get; set; }
        }

        internal static partial class AddedBinder
        {
            [Bind]
            public static partial AddedTarget BindAdded(IQueryCollection query);
        }
        """;

    // ------------------------------------------------------------
    // Cache
    // ------------------------------------------------------------

    [Fact]
    public void UnrelatedEditKeepsModelCached()
    {
        // Arrange & Act
        var result = CompilationHelper.RunIncremental(Source, UnrelatedSource);

        // Assert
        Assert.Equal(result.FirstGeneratedText, result.SecondGeneratedText);
        Assert.NotEmpty(result.OutputReasons);
        Assert.DoesNotContain(result.OutputReasons, static x => x.IsChanged());
    }

    [Fact]
    public void TargetEditRebuildsModel()
    {
        // Arrange & Act
        var result = CompilationHelper.RunIncremental(Source, AddedTargetSource);

        // Assert
        Assert.Contains(result.OutputReasons, static x => x.IsChanged());
    }
}
