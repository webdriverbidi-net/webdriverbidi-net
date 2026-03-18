namespace WebDriverBiDi;

[TestFixture]
public class StringEnumValueConverterTests
{
    [Test]
    public void ShouldConvertEnumValue()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.That(converter.GetString(BasicEnum.FirstValue), Is.EqualTo("firstvalue"));
    }

    [Test]
    public void ShouldConvertEnumValueWithCustomSerializedValue()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.That(converter.GetString(BasicEnum.SecondValue), Is.EqualTo("second-value"));
    }

    [Test]
    public void ShouldConvertStringToBasicValue()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.That(converter.GetValue("firstvalue"), Is.EqualTo(BasicEnum.FirstValue));
    }

    [Test]
    public void ShouldConvertStringToCustomValue()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.That(converter.GetValue("second-value"), Is.EqualTo(BasicEnum.SecondValue));
    }

    [Test]
    public void ShouldConvertInvalidStringValueWhenDefaultAttributeSet()
    {
        StringEnumValueConverter<EnumWithDefault> converter = new();
        EnumWithDefault value = converter.GetValue("invalid");
        Assert.That(value, Is.EqualTo(EnumWithDefault.DefaultValue));
    }

    [Test]
    public void ConvertInvalidStringValueThrows()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.That(() => converter.GetValue("invalid"), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public void TryConvertInvalidStringValueReturnsFalseWithNoDefaultSpecified()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.That(converter.TryGetValue("invalid", out _), Is.False);
    }

    [Test]
    public void TryConvertInvalidStringValueReturnsFalseWithDefaultSpecified()
    {
        StringEnumValueConverter<EnumWithDefault> converter = new();
        Assert.That(converter.TryGetValue("invalid", out _), Is.False);
    }

    [Test]
    public void ConvertInvalidEnumValueThrows()
    {
        StringEnumValueConverter<FlagEnum> converter = new();
        Assert.That(() => converter.GetString(FlagEnum.FirstValue | FlagEnum.SecondValue), Throws.InstanceOf<ArgumentException>());
    }

    private enum BasicEnum
    {
        FirstValue,

        [StringEnumValue("second-value")]
        SecondValue
    }

    [StringEnumUnmatchedValue<EnumWithDefault>(DefaultValue)]
    private enum EnumWithDefault
    {
        [StringEnumValue("default-value")]
        DefaultValue,

        [StringEnumValue("non-default-value")]
        NonDefaultValue
    }

    [Flags]
    private enum FlagEnum
    {
        FirstValue = 1,
        SecondValue = 2
    }
}
