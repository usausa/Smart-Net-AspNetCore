namespace Smart.AspNetCore.Binders;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

//--------------------------------------------------------------------------------
// Binder / Model
//--------------------------------------------------------------------------------

internal static class CustomValueConverter
{
    // ReSharper disable once UnusedMember.Global
    public static CustomValue ToCustomValue(ReadOnlySpan<char> value) => new(value.ToString());
}

internal static partial class TypeComboBinder
{
    [Bind]
    [BindConverter(typeof(CustomValueConverter))]
    public static partial TypeComboRequest BindCombo(IQueryCollection query);
}

// ReSharper disable once NotAccessedPositionalProperty.Global
internal sealed record CustomValue(string Text);

internal sealed class TypeComboRequest
{
    public int? NullableInt { get; set; }

    public SampleColor? NullableEnum { get; set; }

    public Guid Id { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly Time { get; set; }

    public TimeSpan Duration { get; set; }

    public DateTimeOffset Offset { get; set; }

    public int[] Ints { get; set; } = [];

    public int?[] NullableInts { get; set; } = [];

    public CustomValue[] Customs { get; set; } = [];
}

//--------------------------------------------------------------------------------
// Test
//--------------------------------------------------------------------------------

public sealed class TypeComboBinderTest
{
    private static readonly string[] IntInput = ["1", "2"];

    private static readonly string[] CustomInput = ["a", "b"];

    private static readonly int[] ExpectedInts = [1, 2];

    private static readonly int?[] ExpectedNullableInts = [1, 2];

    private static readonly CustomValue[] ExpectedCustoms = [new("a"), new("b")];

    [Fact]
    public void NullableAndSpecialScalarsBind()
    {
        var id = new Guid("11111111-2222-3333-4444-555555555555");
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["NullableInt"] = "42",
            ["NullableEnum"] = "Green",
            ["Id"] = id.ToString(),
            ["Date"] = "2024-01-15",
            ["Time"] = "13:45:00",
            ["Duration"] = "01:02:03",
            ["Offset"] = "2024-01-15T10:00:00+09:00"
        });

        var result = TypeComboBinder.BindCombo(query);

        Assert.Equal(42, result.NullableInt);
        Assert.Equal(SampleColor.Green, result.NullableEnum);
        Assert.Equal(id, result.Id);
        Assert.Equal(new DateOnly(2024, 1, 15), result.Date);
        Assert.Equal(new TimeOnly(13, 45), result.Time);
        Assert.Equal(new TimeSpan(1, 2, 3), result.Duration);
        Assert.Equal(new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.FromHours(9)), result.Offset);
    }

    [Fact]
    public void ArraysAndCustomConverterArrayBind()
    {
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["Ints"] = new(IntInput),
            ["NullableInts"] = new(IntInput),
            ["Customs"] = new(CustomInput)
        });

        var result = TypeComboBinder.BindCombo(query);

        Assert.Equal(ExpectedInts, result.Ints);
        Assert.Equal(ExpectedNullableInts, result.NullableInts);
        Assert.Equal(ExpectedCustoms, result.Customs);
    }
}
