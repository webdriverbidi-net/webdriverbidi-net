namespace WebDriverBiDi.Script;

using System.Text.Json;

public class RegularExpressionValueTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "pattern": "myPattern"
                      }
                      """;
        RegularExpressionValue regexProperties = JsonSerializer.Deserialize<RegularExpressionValue>(json);

        Assert.Equal("myPattern", regexProperties.Pattern);
        Assert.Null(regexProperties.Flags);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "pattern": "myPattern"
                      }
                      """;
        RegularExpressionValue regexProperties = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        RegularExpressionValue copy = regexProperties;
        Assert.Equal(regexProperties, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingPatternThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RegularExpressionValue>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidPatternTypeThrows()
    {
        string json = """
                      {
                        "pattern": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RegularExpressionValue>(json));
    }

    [Fact]
    public void TestCanDeserializeWithOptionalFlags()
    {
        string json = """
                      {
                        "pattern": "myPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue regexProperties = JsonSerializer.Deserialize<RegularExpressionValue>(json);

        Assert.Equal("myPattern", regexProperties.Pattern);
        Assert.NotNull(regexProperties.Flags);
        Assert.Equal("gi", regexProperties.Flags);
    }

    [Fact]
    public void TestDeserializeWithInvalidFlagsTypeThrows()
    {
        string json = """
                      {
                        "pattern": "myPattern",
                        "flags": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RegularExpressionValue>(json));
    }

    [Fact]
    public void TestEquality()
    {
        LocalValue regexLocalValue = LocalValue.RegExp("myPattern", "gi");
        LocalArgumentValue primitiveValue = (LocalArgumentValue)regexLocalValue;
        Assert.NotNull(primitiveValue.Value);
        RegularExpressionValue expectedRegexValue = (RegularExpressionValue)primitiveValue.Value;

        string json = """
                      {
                        "pattern": "myPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.Equal(expectedRegexValue, actualRegexValue);
    }

    [Fact]
    public void TestInequalityWithDifferingPatterns()
    {
        LocalValue regexLocalValue = LocalValue.RegExp("myPattern", "gi");
        LocalArgumentValue primitiveValue = (LocalArgumentValue)regexLocalValue;
        Assert.NotNull(primitiveValue.Value);
        RegularExpressionValue expectedRegexValue = (RegularExpressionValue)primitiveValue.Value;

        string json = """
                      {
                        "pattern": "notMyPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.NotEqual(expectedRegexValue, actualRegexValue);
    }

    [Fact]
    public void TestInequalityWithDifferingPatternsAndNullFlags()
    {
        LocalValue regexLocalValue = LocalValue.RegExp("myPattern");
        LocalArgumentValue primitiveValue = (LocalArgumentValue)regexLocalValue;
        Assert.NotNull(primitiveValue.Value);
        RegularExpressionValue expectedRegexValue = (RegularExpressionValue)primitiveValue.Value;

        string json = """
                      {
                        "pattern": "notMyPattern"
                      }
                      """;
        RegularExpressionValue actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.NotEqual(expectedRegexValue, actualRegexValue);
    }

    [Fact]
    public void TestInequalityWithDifferingFlags()
    {
        LocalValue regexLocalValue = LocalValue.RegExp("myPattern", "g");
        LocalArgumentValue primitiveValue = (LocalArgumentValue)regexLocalValue;
        Assert.NotNull(primitiveValue.Value);
        RegularExpressionValue expectedRegexValue = (RegularExpressionValue)primitiveValue.Value;

        string json = """
                      {
                        "pattern": "myPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.NotEqual(expectedRegexValue, actualRegexValue);
    }

    [Fact]
    public void TestInequalityWithNull()
    {
        string json = """
                      {
                        "pattern": "myPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.False(actualRegexValue.Equals(null));
    }

    [Fact]
    public void TestInequalityWithInvalidObjectType()
    {
        string json = """
                      {
                        "pattern": "myPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.False(actualRegexValue.Equals("invalid"));
    }
}
