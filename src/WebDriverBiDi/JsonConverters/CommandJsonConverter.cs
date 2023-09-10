namespace WebDriverBiDi.JsonConverters;

using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Protocol;

public class CommandJsonConverter : JsonConverter<Command>
{
    public override Command? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, Command value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("id");
        writer.WriteNumberValue(value.CommandId);
        writer.WritePropertyName("method");
        writer.WriteStringValue(value.CommandName);
        writer.WritePropertyName("params");
        string serializedParams = JsonSerializer.Serialize(value.CommandParameters, value.CommandParameters.GetType());
        writer.WriteRawValue(serializedParams);
        foreach (KeyValuePair<string, object?> pair in value.AdditionalData)
        {
            writer.WritePropertyName(pair.Key);
            writer.WriteRawValue(JsonSerializer.Serialize(pair.Value));
        }
        writer.WriteEndObject();
    }
}