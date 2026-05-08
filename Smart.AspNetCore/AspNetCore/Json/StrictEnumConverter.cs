namespace Smart.AspNetCore.Json;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class StrictEnumConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(EnumConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

#pragma warning disable CA1812
    private sealed class EnumConverter<T> : JsonConverter<T>
        where T : Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString()!;
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new JsonException($"Invalid enum value. value=[{value}]");
            }

            return (T)Enum.Parse(typeToConvert, value);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
#pragma warning restore CA1812
}
