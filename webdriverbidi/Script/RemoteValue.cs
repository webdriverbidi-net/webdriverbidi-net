namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using JsonConverters;

[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(RemoteValueJsonConverter))]
public class RemoteValue
{
    private string valueType;
    private string? handle; 
    private uint? internalId;
    private object? valueObject;

    internal RemoteValue(string valueType)
    {
        this.valueType = valueType;
    }

    [JsonProperty("type")]
    public string Type { get => this.valueType; internal set => this.valueType = value; }

    [JsonProperty("handle")]
    public string? Handle { get => this.handle; internal set => this.handle = value; }

    [JsonProperty("internalId")]
    public uint? InternalId { get => this.internalId; internal set => this.internalId = value; }

    [JsonProperty("value")]
    public object? Value { get => this.valueObject; internal set => this.valueObject = value; }
    
    public bool HasValue => this.valueObject is not null;

    public bool IsPrimitive => this.valueType == "string" || this.valueType == "number" || this.valueType == "boolean" || this.valueType == "bigint" || this.valueType == "null" || this.valueType == "undefined";

    public T? ValueAs<T>()
    {
        var result = default(T);
        Type type = typeof(T);
        if (this.valueObject == null)
        {
            if (type.IsValueType && (Nullable.GetUnderlyingType(type) == null))
            {
                throw new WebDriverBidiException("RemoteValue has null value, but desired type is a value type");
            }
        }
        else if (!type.IsInstanceOfType(this.valueObject))
        {
            throw new WebDriverBidiException("RemoteValue could not be cast to the desired type");
        }
        else
        {
            result = (T)this.valueObject;
        }

        return result;
    }
}