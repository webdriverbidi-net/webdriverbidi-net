namespace WebDriverBidi.Script;

using System.Globalization;
using System.Numerics;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class LocalValue : ArgumentValue
{
    private string argType;
    private object? argValue;

    internal LocalValue(string argType)
    {
        this.argType = argType;
    }

    [JsonProperty("type")]
    public string Type { get => this.argType; internal set => this.argType = value; }

    public object? Value { get => this.argValue; internal set => this.argValue = value; }

    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    internal object? SerializableValue
    {
        get
        {
            if (this.argType == "number")
            {
                return this.GetSerializedNumericValue();
            }

            if (this.argType == "bigint")
            {
                BigInteger? bigintValue = this.argValue as BigInteger?;
                if (bigintValue is not null && bigintValue.HasValue)
                {
                    return bigintValue.Value.ToString(CultureInfo.InvariantCulture);
                }
            }

            if (this.argType == "date")
            {
                DateTime? dateValue = this.argValue as DateTime?;
                if (dateValue is not null && dateValue.HasValue)
                {
                    return dateValue.Value.ToString("YYYY-MM-ddTHH:mm:ss.fffzzz");
                }
            }

            if (this.argType == "map" || this.argType == "object")
            {
                Dictionary<LocalValue, LocalValue>? dictionaryValue = this.argValue as Dictionary<LocalValue, LocalValue>;
                if (dictionaryValue is not null)
                {
                    Dictionary<string, LocalValue> serializableDictionary = new Dictionary<string, LocalValue>();
                    foreach (var pair in dictionaryValue)
                    {
                        serializableDictionary[JsonConvert.SerializeObject(pair.Key)] = pair.Value;
                    }

                    return serializableDictionary;
                }
            }

            return this.argValue;
        }
    }

    private object? GetSerializedNumericValue()
    {
        double? doubleValue = this.argValue as double?;
        if (doubleValue is not null && doubleValue.HasValue)
        {
            if (double.IsNaN(doubleValue.Value))
            {
                return "NaN";
            }
            else if (double.IsPositiveInfinity(doubleValue.Value))
            {
                return "Infinity";
            }
            else if (double.IsNegativeInfinity(doubleValue.Value))
            {
                return "-Infinity";
            }
        }

        decimal? decimalValue = this.argValue as decimal?;
        if (decimalValue is not null && decimalValue.HasValue && decimalValue.Value == decimal.Negate(decimal.Zero))
        {
            return "-0";
        }

        return this.argValue;
    }
    
    public static LocalValue Undefined => new LocalValue("undefined");
    public static LocalValue Null => new LocalValue("null");
    public static LocalValue NaN =>  new LocalValue("number") { argValue = double.NaN };
    public static LocalValue NegativeZero => new LocalValue("number") { argValue = decimal.Negate(decimal.Zero) };
    public static LocalValue Infinity => new LocalValue("number") { argValue = double.PositiveInfinity };
    public static LocalValue NegativeInfinity => new LocalValue("number") { argValue = double.NegativeInfinity };
    public static LocalValue String(string stringValue) => new LocalValue("string") { argValue = stringValue };
    public static LocalValue Number(int numericValue) => new LocalValue("number") { argValue = numericValue };
    public static LocalValue Number(long numericValue) => new LocalValue("number") { argValue = numericValue };
    public static LocalValue Number(double numericValue) => new LocalValue("number") { argValue = numericValue };
    public static LocalValue Boolean(bool boolValue) => new LocalValue("boolean") { argValue = boolValue };
    public static LocalValue BigInt(BigInteger bigIntValue) => new LocalValue("bigint") { argValue = bigIntValue };
    public static LocalValue Date(DateTime dateTimeValue) => new LocalValue("date") { argValue = dateTimeValue };
    public static LocalValue Array(List<LocalValue> arrayValue) => new LocalValue("array") { argValue = arrayValue };
    public static LocalValue Set(List<LocalValue> arrayValue) => new LocalValue("set") { argValue = arrayValue };
    public static LocalValue Map(Dictionary<string, LocalValue> mapValue) => new LocalValue("map") { argValue = mapValue };
    public static LocalValue Map(Dictionary<LocalValue, LocalValue> mapValue) => new LocalValue("map") { argValue = mapValue };
    public static LocalValue Object(Dictionary<string, LocalValue> mapValue) => new LocalValue("object") { argValue = mapValue };
    public static LocalValue Object(Dictionary<LocalValue, LocalValue> mapValue) => new LocalValue("object") { argValue = mapValue };
    public static LocalValue RegExp(string pattern, string? flags = null) => new LocalValue("regexp") { argValue = new RegularExpressionProperties(pattern, flags) };
}