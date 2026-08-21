namespace Smart.AspNetCore.Binders;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

//--------------------------------------------------------------------------------
// Model / Binder
//--------------------------------------------------------------------------------

internal static partial class StrictBinder
{
    [Bind(Strict = true)]
    public static partial StrictRequest BindStrict(IQueryCollection query);
}

internal sealed class StrictRequest
{
    public int Page { get; set; }

    public int[] Values { get; set; } = [];
}

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class StrictBinderTest
{
    private static readonly string[] ValidArrayInput = ["1", "2"];

    private static readonly string[] InvalidArrayInput = ["1", "abc"];

    private static readonly int[] ExpectedValues = [1, 2];

    [Fact]
    public void ValidValueBindsCorrectly()
    {
        var query = new QueryCollection(new Dictionary<string, StringValues> { ["Page"] = "5" });

        var result = StrictBinder.BindStrict(query);

        Assert.Equal(5, result.Page);
    }

    [Fact]
    public void InvalidValueThrowsFormatException()
    {
        var query = new QueryCollection(new Dictionary<string, StringValues> { ["Page"] = "abc" });

        Assert.Throws<FormatException>(() => StrictBinder.BindStrict(query));
    }

    [Fact]
    public void ValidArrayBindsCorrectly()
    {
        var query = new QueryCollection(new Dictionary<string, StringValues> { ["Values"] = [with(ValidArrayInput)] });

        var result = StrictBinder.BindStrict(query);

        Assert.Equal(ExpectedValues, result.Values);
    }

    [Fact]
    public void InvalidArrayElementThrowsFormatException()
    {
        var query = new QueryCollection(new Dictionary<string, StringValues> { ["Values"] = [with(InvalidArrayInput)] });

        Assert.Throws<FormatException>(() => StrictBinder.BindStrict(query));
    }
}
