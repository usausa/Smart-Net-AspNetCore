namespace Smart.AspNetCore.Binders;

public sealed class DefaultStringConverterTest
{
    [Fact]
    public void ToInt32ReturnsZeroForInvalidInput()
    {
        Assert.Equal(0, DefaultStringConverter.ToInt32("abc"));
    }

    [Fact]
    public void ToGuidReturnsEmptyForInvalidInput()
    {
        Assert.Equal(Guid.Empty, DefaultStringConverter.ToGuid("not-a-guid"));
    }

    [Fact]
    public void ToDoubleParsesWithInvariantCulture()
    {
        Assert.Equal(1.5d, DefaultStringConverter.ToDouble("1.5"));
    }

    [Fact]
    public void TryToInt32ReturnsFalseForInvalidInput()
    {
        Assert.False(DefaultStringConverter.TryToInt32("abc", out var result));
        Assert.Equal(0, result);
    }

    [Fact]
    public void TryToInt32ReturnsTrueForValidInput()
    {
        Assert.True(DefaultStringConverter.TryToInt32("42", out var result));
        Assert.Equal(42, result);
    }

    [Fact]
    public void TryToGuidReturnsFalseForInvalidInput()
    {
        Assert.False(DefaultStringConverter.TryToGuid("not-a-guid", out _));
    }

    [Fact]
    public void TryToCharReturnsFalseForMultiCharInput()
    {
        Assert.False(DefaultStringConverter.TryToChar("ab", out _));
    }

    [Fact]
    public void TryToEnumReturnsFalseForInvalidName()
    {
        Assert.False(DefaultStringConverter.TryToEnum<DayOfWeek>("NotADay", out _));
    }

    [Fact]
    public void TryToEnumReturnsTrueForValidName()
    {
        Assert.True(DefaultStringConverter.TryToEnum<DayOfWeek>("Monday", out var result));
        Assert.Equal(DayOfWeek.Monday, result);
    }

    [Fact]
    public void TryToDoubleParsesWithInvariantCulture()
    {
        Assert.True(DefaultStringConverter.TryToDouble("1.5", out var result));
        Assert.Equal(1.5d, result);
    }

    [Fact]
    public void TryToDateOnlyParsesIsoFormatWithInvariantCulture()
    {
        Assert.True(DefaultStringConverter.TryToDateOnly("2024-01-15", out var result));
        Assert.Equal(new DateOnly(2024, 1, 15), result);
    }
}
