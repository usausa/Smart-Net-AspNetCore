namespace Smart.AspNetCore.Json;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public abstract class DateTimeFormatConverter : JsonConverter<DateTime>
{
    public string Format { get; }

    protected DateTimeFormatConverter(string format)
    {
        Format = format;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token parsing DateTime. token=[{reader.TokenType}], format=[{Format}]");
        }

        var value = reader.GetString();
        if ((value is null) || !DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            throw new JsonException($"Invalid date format. value=[{value}], format=[{Format}]");
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
