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
        try
        {
            return DateTime.ParseExact(reader.GetString()!, Format, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            throw new JsonException($"Invalid date format. format=[{Format}]");
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
