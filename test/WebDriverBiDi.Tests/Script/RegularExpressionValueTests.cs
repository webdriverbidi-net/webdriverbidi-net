namespace WebDriverBiDi.Script;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

[TestFixture]
public class RegularExpressionValueTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "pattern": "myPattern"
                      }
                      """;
        RegularExpressionValue? regexProperties = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.That(regexProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(regexProperties.Pattern, Is.EqualTo("myPattern"));
            Assert.That(regexProperties.Flags, Is.Null);
        });
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "pattern": "myPattern"
                      }
                      """;
        RegularExpressionValue? regexProperties = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.That(regexProperties, Is.Not.Null);
        RegularExpressionValue copy = regexProperties with { };
        Assert.That(copy, Is.EqualTo(regexProperties));
    }

    [Test]
    public void TestDeserializeWithMissingPatternThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<RegularExpressionValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidPatternTypeThrows()
    {
        string json = """
                      {
                        "pattern": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RegularExpressionValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalFlags()
    {
        string json = """
                      {
                        "pattern": "myPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue? regexProperties = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.That(regexProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(regexProperties.Pattern, Is.EqualTo("myPattern"));
            Assert.That(regexProperties.Flags, Is.Not.Null);
            Assert.That(regexProperties.Flags, Is.EqualTo("gi"));
        });
    }

    [Test]
    public void TestDeserializeWithInvalidFlagsTypeThrows()
    {
        string json = """
                      {
                        "pattern": "myPattern",
                        "flags": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RegularExpressionValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestEquality()
    {
        RegularExpressionValue expectedRegexValue = new("myPattern", "gi");

        string json = """
                      {
                        "pattern": "myPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue? actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.That(actualRegexValue, Is.EqualTo(expectedRegexValue));
    }

    [Test]
    public void TestInequalityWithDifferingPatterns()
    {
        RegularExpressionValue expectedRegexValue = new("myPattern", "gi");

        string json = """
                      {
                        "pattern": "notMyPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue? actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.That(actualRegexValue, Is.Not.EqualTo(expectedRegexValue));
    }

    [Test]
    public void TestInequalityWithDifferingPatternsAndNullFlags()
    {
        RegularExpressionValue expectedRegexValue = new("myPattern");

        string json = """
                      {
                        "pattern": "notMyPattern"
                      }
                      """;
        RegularExpressionValue? actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.That(actualRegexValue, Is.Not.EqualTo(expectedRegexValue));
    }

    [Test]
    public void TestInequalityWithDifferingFlags()
    {
        RegularExpressionValue expectedRegexValue = new("myPattern", "g");

        string json = """
                      {
                        "pattern": "myPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue? actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.That(actualRegexValue, Is.Not.EqualTo(expectedRegexValue));
    }

    [Test]
    [SuppressMessage("Assertion", "NUnit2010")]
    public void TestInequalityWithNull()
    {
        string json = """
                      {
                        "pattern": "myPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue? actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.That(actualRegexValue, Is.Not.Null);
        Assert.That(actualRegexValue.Equals(null), Is.False);
    }

    [Test]
    [SuppressMessage("Assertion", "NUnit2010")]
    public void TestInequalityWithInvalidObjectType()
    {
         string json = """
                      {
                        "pattern": "myPattern",
                        "flags": "gi"
                      }
                      """;
        RegularExpressionValue? actualRegexValue = JsonSerializer.Deserialize<RegularExpressionValue>(json);
        Assert.That(actualRegexValue, Is.Not.Null);
        Assert.That(actualRegexValue!.Equals("invalid"), Is.False);
    }
}
