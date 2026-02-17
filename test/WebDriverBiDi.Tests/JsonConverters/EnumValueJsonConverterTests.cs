namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;

[TestFixture]
public class EnumValueJsonConverterTests
{
    [Test]
    public void ShouldSerializeValue()
    {
        string json = JsonSerializer.Serialize(BasicEnum.FirstValue);
        Assert.That(json, Is.EqualTo("\"firstvalue\""));
    }

    [Test]
    public void ShouldSerializeValueWithCustomSerializedValue()
    {
        string json = JsonSerializer.Serialize(BasicEnum.SecondValue);
        Assert.That(json, Is.EqualTo("\"second-value\""));
    }

    [Test]
    public void ShouldDeserializeBasicValue()
    {
        BasicEnum? value = JsonSerializer.Deserialize<BasicEnum>("\"firstvalue\"");
        Assert.That(value, Is.EqualTo(BasicEnum.FirstValue));
    }

    [Test]
    public void ShouldDeserializeCustomValue()
    {
        BasicEnum? value = JsonSerializer.Deserialize<BasicEnum>("\"second-value\"");
        Assert.That(value, Is.EqualTo(BasicEnum.SecondValue));
    }

    [Test]
    public void ShouldDeserializeInvalidValueWhenAttributeSet()
    {
        EnumWithDefault? value = JsonSerializer.Deserialize<EnumWithDefault>("\"invalid\"");
        Assert.That(value, Is.EqualTo(EnumWithDefault.DefaultValue));
    }

    [Test]
    public void DeserializeNonStringValueThrows()
    {
        Assert.That(() => JsonSerializer.Deserialize<BasicEnum>("1"), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo($"Deserialization error reading enumerated string value"));
    }

    [Test]
    public void DeserializeInvalidValueThrows()
    {
        Assert.That(() => JsonSerializer.Deserialize<BasicEnum>("\"invalid\""), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo($"Deserialization error: value 'invalid' is not valid for enum type WebDriverBiDi.JsonConverters.EnumValueJsonConverterTests+BasicEnum"));
    }

    [JsonConverter(typeof(EnumValueJsonConverter<BasicEnum>))]
    private enum BasicEnum
    {
        FirstValue,

        [JsonEnumValue("second-value")]
        SecondValue
    }

    [JsonConverter(typeof(EnumValueJsonConverter<EnumWithDefault>))]
    [JsonEnumUnmatchedValue<EnumWithDefault>(DefaultValue)]
    private enum EnumWithDefault
    {
        [JsonEnumValue("default-value")]
        DefaultValue,

        [JsonEnumValue("non-default-value")]
        NonDefaultValue
    }
}
