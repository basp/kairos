namespace Kairos;

using System.Text.Json;
using System.Text.Json.Serialization;

internal class GuidJsonConverter : JsonConverter<Guid>
{
    public override Guid Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return Guid.TryParseExact(
            value,
            "N",
            out var result)
            ? result
            : throw new JsonException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        Guid value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(
            value.ToString("N"));
    }
}