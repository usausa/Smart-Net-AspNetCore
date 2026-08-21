namespace Smart.AspNetCore.Binders;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

//--------------------------------------------------------------------------------
// Binder / Model
//--------------------------------------------------------------------------------

internal static partial class EnumBinder
{
    [Bind]
    public static partial EnumRequest BindEnum(IQueryCollection query);
}

internal enum SampleColor
{
    Red,
    Green,
    Blue
}

internal sealed class EnumRequest
{
    public SampleColor Single { get; set; }

    public SampleColor[] Colors { get; set; } = [];

    public SampleColor?[] NullableColors { get; set; } = [];
}

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class EnumBinderTest
{
    private static readonly string[] ColorInput = ["Red", "Blue"];

    private static readonly SampleColor[] ExpectedColors = [SampleColor.Red, SampleColor.Blue];

    private static readonly SampleColor?[] ExpectedNullableColors = [SampleColor.Red, SampleColor.Blue];

    [Fact]
    public void ScalarEnumBinds()
    {
        var query = new QueryCollection(new Dictionary<string, StringValues> { ["Single"] = "Green" });

        var result = EnumBinder.BindEnum(query);

        Assert.Equal(SampleColor.Green, result.Single);
    }

    [Fact]
    public void EnumArrayBindsAllValues()
    {
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["Colors"] = [with(ColorInput)]
        });

        var result = EnumBinder.BindEnum(query);

        Assert.Equal(ExpectedColors, result.Colors);
    }

    [Fact]
    public void NullableEnumArrayBindsAllValues()
    {
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["NullableColors"] = [with(ColorInput)]
        });

        var result = EnumBinder.BindEnum(query);

        Assert.Equal(ExpectedNullableColors, result.NullableColors);
    }
}
