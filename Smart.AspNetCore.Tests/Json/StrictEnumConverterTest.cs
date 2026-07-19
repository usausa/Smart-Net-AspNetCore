namespace Smart.AspNetCore.Json;

using System.Text.Json;

public sealed class StrictEnumConverterTest
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public enum Color
    {
        Red,
        Green,
        Blue
    }

    [Flags]
    public enum AccessModes
    {
        None = 0,
        Read = 1,
        Write = 2
    }

    [Fact]
    public void ReadValidNameReturnsEnum()
    {
        var result = JsonSerializer.Deserialize<Color>("\"Green\"", Options);

        Assert.Equal(Color.Green, result);
    }

    [Fact]
    public void WriteEnumProducesName()
    {
        var json = JsonSerializer.Serialize(Color.Blue, Options);

        Assert.Equal("\"Blue\"", json);
    }

    [Fact]
    public void ReadInvalidNameThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Color>("\"Purple\"", Options));
    }

    [Fact]
    public void ReadWrongCaseThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Color>("\"green\"", Options));
    }

    [Fact]
    public void ReadNumberTokenThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Color>("1", Options));
    }

    [Fact]
    public void ReadNullTokenThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Color>("null", Options));
    }

    [Fact]
    public void ReadFlagsCombinationStringThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<AccessModes>("\"Read, Write\"", Options));
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new StrictEnumConverter());
        return options;
    }
}
