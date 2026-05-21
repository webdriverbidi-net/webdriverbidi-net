namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;

public class EnumValueJsonConverterTests
{
    [Fact]
    public void ShouldSerializeValue()
    {
        string json = JsonSerializer.Serialize(BasicEnum.FirstValue);
        Assert.Equal("\"firstvalue\"", json);
    }

    [Fact]
    public void ShouldSerializeValueWithCustomSerializedValue()
    {
        string json = JsonSerializer.Serialize(BasicEnum.SecondValue);
        Assert.Equal("\"second-value\"", json);
    }

    [Fact]
    public void ShouldDeserializeBasicValue()
    {
        BasicEnum? value = JsonSerializer.Deserialize<BasicEnum>("\"firstvalue\"");
        Assert.Equal(BasicEnum.FirstValue, value);
    }

    [Fact]
    public void ShouldDeserializeCustomValue()
    {
        BasicEnum? value = JsonSerializer.Deserialize<BasicEnum>("\"second-value\"");
        Assert.Equal(BasicEnum.SecondValue, value);
    }

    [Fact]
    public void ShouldDeserializeInvalidValueWhenAttributeSet()
    {
        EnumWithDefault? value = JsonSerializer.Deserialize<EnumWithDefault>("\"invalid\"");
        Assert.Equal(EnumWithDefault.DefaultValue, value);
    }

    [Fact]
    public void DeserializeNonStringValueThrows()
    {
        Assert.Equal($"Deserialization error reading enumerated string value", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BasicEnum>("1")).Message);
    }

    [Fact]
    public void DeserializeInvalidValueThrows()
    {
        Assert.Equal($"Deserialization error: value 'invalid' is not valid for enum type BasicEnum", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BasicEnum>("\"invalid\"")).Message);
    }

    [Fact]
    public void SerializeInvalidValueThrows()
    {
        Assert.StartsWith("Serialization error: value", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Serialize(FlagEnum.FirstValue | FlagEnum.SecondValue)).Message);
    }

    [JsonConverter(typeof(EnumValueJsonConverter<BasicEnum>))]
    private enum BasicEnum
    {
        FirstValue,

        [StringEnumValue("second-value")]
        SecondValue
    }

    [JsonConverter(typeof(EnumValueJsonConverter<EnumWithDefault>))]
    [StringEnumUnmatchedValue<EnumWithDefault>(DefaultValue)]
    private enum EnumWithDefault
    {
        [StringEnumValue("default-value")]
        DefaultValue,

        [StringEnumValue("non-default-value")]
        NonDefaultValue
    }

    [JsonConverter(typeof(EnumValueJsonConverter<FlagEnum>))]
    [Flags]
    private enum FlagEnum
    {
        FirstValue = 1,
        SecondValue = 2
    }
}
