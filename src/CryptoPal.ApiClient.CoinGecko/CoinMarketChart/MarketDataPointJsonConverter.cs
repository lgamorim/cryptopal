using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoPal.ApiClient.CoinGecko.CoinMarketChart;

/// <summary>
/// Reads a CoinGecko market data row, which is serialized as a two-element JSON
/// array of <c>[unixTimeMilliseconds, value]</c>, into a strongly typed <see cref="MarketDataPoint"/>.
/// </summary>
public class MarketDataPointJsonConverter : JsonConverter<MarketDataPoint>
{
    public override MarketDataPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected the start of an array for a {nameof(MarketDataPoint)}.");
        }

        var timestampMs = (long)ReadNumber(ref reader, "timestamp");
        var value = ReadNumber(ref reader, "value");

        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException($"Expected exactly two elements for a {nameof(MarketDataPoint)}.");
        }

        return new MarketDataPoint(timestampMs, value);
    }

    public override void Write(Utf8JsonWriter writer, MarketDataPoint value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.TimestampMs);
        writer.WriteNumberValue(value.Value);
        writer.WriteEndArray();
    }

    private static decimal ReadNumber(ref Utf8JsonReader reader, string element)
    {
        if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Expected a numeric {element} for a {nameof(MarketDataPoint)}.");
        }

        return reader.GetDecimal();
    }
}
