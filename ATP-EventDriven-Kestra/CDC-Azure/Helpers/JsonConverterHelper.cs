using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CDC_Azure.Helpers
{
    public static class JsonConverterHelper
    {
        public static JsonSerializerOptions GetDefaultOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new StringToDecimalConverter(),
                    new UnixMillisecondsToDateTimeConverter()
                }
            };
            return options;
        }
    }

    public class UnixMillisecondsToDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                var milliseconds = reader.GetInt64();
                return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).LocalDateTime;
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (long.TryParse(str, out var milliseconds))
                    return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).LocalDateTime;
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var milliseconds = new DateTimeOffset(value).ToUnixTimeMilliseconds();
            writer.WriteNumberValue(milliseconds);
        }
    }

    public class StringToDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (decimal.TryParse(str, out var value))
                    return value;
                else
                    throw new JsonException($"Invalid decimal value: {str}");
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
