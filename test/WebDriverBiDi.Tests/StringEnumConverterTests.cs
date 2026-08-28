namespace WebDriverBiDi;

using System.Reflection;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

public class StringEnumValueConverterTests
{
    [Fact]
    public void NoEnumUsingLibraryConverterCarriesJsonStringEnumMemberName()
    {
        // The library's EnumValueJsonConverter honors [StringEnumValue], not System.Text.Json's
        // [JsonStringEnumMemberName], which it ignores. A member decorated with the latter would
        // serialize with the wrong (attribute-ignored) string, so no field of an enum that uses the
        // library converter may carry it.
        List<string> offenders = new();
        List<Type> scannedEnums = new();
        foreach (Type type in typeof(ErrorCode).Assembly.GetTypes())
        {
            if (!type.IsEnum)
            {
                continue;
            }

            Type? converterType = type.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType;
            if (converterType is null || !converterType.IsGenericType || converterType.GetGenericTypeDefinition() != typeof(EnumValueJsonConverter<>))
            {
                continue;
            }

            scannedEnums.Add(type);
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>() is not null)
                {
                    offenders.Add($"{type.Name}.{field.Name}");
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            $"Enums using EnumValueJsonConverter must use [StringEnumValue], not [JsonStringEnumMemberName]. Offenders: {string.Join(", ", offenders)}");

        // Guard against the check passing vacuously: the enums whose members were previously
        // mis-attributed must actually be among those scanned.
        Assert.Contains(typeof(WebDriverBiDi.Emulation.OverflowBlockMediaFeatureValue), scannedEnums);
        Assert.Contains(typeof(WebDriverBiDi.Emulation.ScriptingMediaFeatureValue), scannedEnums);
        Assert.Contains(typeof(WebDriverBiDi.Emulation.PrefersReducedTransparencyMediaFeatureValue), scannedEnums);
    }

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
    public void ShouldEnableNullSerializationForSentinelValue()
    {
        StringEnumValueConverter<EnumWithSentinelNullValue> converter = new();
        Assert.Equal(EnumWithSentinelNullValue.Reset, converter.NullSentinelValue);
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

    [StringEnumNullSentinelValue<EnumWithSentinelNullValue>(Reset)]
    private enum EnumWithSentinelNullValue
    {
        [StringEnumValue("non-null-value")]
        NonNull,

        Reset
    }

    [Flags]
    private enum FlagEnum
    {
        FirstValue = 1,
        SecondValue = 2
    }
}
