using System.Text.Json;
using System.Text.Json.Serialization;

namespace FishingMap.Domain.DTO.Common
{
    [JsonConverter(typeof(OptionalConverterFactory))]
    public readonly struct Optional<T>
    {
        private readonly T _value;

        public bool HasValue { get; }

        public T Value => _value;

        private Optional(T value)
        {
            HasValue = true;
            _value = value;
        }

        public static Optional<T> Of(T value) => new(value);
    }

    public class OptionalConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var innerType = typeToConvert.GetGenericArguments()[0];
            var converterType = typeof(OptionalConverter<>).MakeGenericType(innerType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    internal class OptionalConverter<T> : JsonConverter<Optional<T>>
    {
        public override bool HandleNull => true;

        public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return Optional<T>.Of(default!);

            var value = JsonSerializer.Deserialize<T>(ref reader, options);
            return Optional<T>.Of(value!);
        }

        public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }
            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}
