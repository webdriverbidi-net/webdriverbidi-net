namespace WebDriverBiDi.JsonConverters;

using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class FixedDoubleJsonConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetDouble(out double doubleValue))
        {
            return doubleValue;
        }

        string? stringValue = reader.GetString();
        return string.IsNullOrWhiteSpace(stringValue) ? default : double.Parse(stringValue, CultureInfo.InvariantCulture);
    }
 
    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        string numberAsString = value.ToString("0.0###########################", CultureInfo.InvariantCulture);
        writer.WriteRawValue(numberAsString);
    }
}