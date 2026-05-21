namespace WebDriverBiDi;

public class StringEnumValueConverterTests
{
    [Fact]
    public void ShouldConvertEnumValue()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.Equal("firstvalue", converter.GetString(BasicEnum.FirstValue));
    }

    [Fact]
    public void ShouldConvertEnumValueWithCustomSerializedValue()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.Equal("second-value", converter.GetString(BasicEnum.SecondValue));
    }

    [Fact]
    public void ShouldConvertStringToBasicValue()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.Equal(BasicEnum.FirstValue, converter.GetValue("firstvalue"));
    }

    [Fact]
    public void ShouldConvertStringToCustomValue()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.Equal(BasicEnum.SecondValue, converter.GetValue("second-value"));
    }

    [Fact]
    public void ShouldConvertInvalidStringValueWhenDefaultAttributeSet()
    {
        StringEnumValueConverter<EnumWithDefault> converter = new();
        EnumWithDefault value = converter.GetValue("invalid");
        Assert.Equal(EnumWithDefault.DefaultValue, value);
    }

    [Fact]
    public void ConvertInvalidStringValueThrows()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.ThrowsAny<ArgumentException>(() => converter.GetValue("invalid"));
    }

    [Fact]
    public void TryConvertInvalidStringValueReturnsFalseWithNoDefaultSpecified()
    {
        StringEnumValueConverter<BasicEnum> converter = new();
        Assert.False(converter.TryGetValue("invalid", out _));
    }

    [Fact]
    public void TryConvertInvalidStringValueReturnsFalseWithDefaultSpecified()
    {
        StringEnumValueConverter<EnumWithDefault> converter = new();
        Assert.False(converter.TryGetValue("invalid", out _));
    }

    [Fact]
    public void ConvertInvalidEnumValueThrows()
    {
        StringEnumValueConverter<FlagEnum> converter = new();
        Assert.ThrowsAny<ArgumentException>(() => converter.GetString(FlagEnum.FirstValue | FlagEnum.SecondValue));
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
