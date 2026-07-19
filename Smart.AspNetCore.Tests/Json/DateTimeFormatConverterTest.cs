namespace Smart.AspNetCore.Json;

using System.Text.Json;

public sealed class DateTimeFormatConverterTest
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    [Fact]
    public void ReadValidFormatReturnsDateTime()
    {
        var result = JsonSerializer.Deserialize<DateTime>("\"2024-01-15\"", Options);

        Assert.Equal(new DateTime(2024, 1, 15), result);
    }

    [Fact]
    public void WriteProducesFormattedString()
    {
        var json = JsonSerializer.Serialize(new DateTime(2024, 1, 15), Options);

        Assert.Equal("\"2024-01-15\"", json);
    }

    [Fact]
    public void ReadInvalidFormatThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"15/01/2024\"", Options));
    }

    [Fact]
    public void ReadNumberTokenThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTime>("20240115", Options));
    }

    [Fact]
    public void ReadNullTokenThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTime>("null", Options));
    }

    [Fact]
    public void RoundTripPreservesValue()
    {
        var original = new DateTime(2023, 12, 31);

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<DateTime>(json, Options);

        Assert.Equal(original, restored);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new TestDateConverter());
        return options;
    }

    private sealed class TestDateConverter : DateTimeFormatConverter
    {
        public TestDateConverter()
            : base("yyyy-MM-dd")
        {
        }
    }
}
