namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class RegExpRemoteValueTests
{
    [Test]
    public void TestCanDeserializeRegExpRemoteValue()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": {
                          "pattern": "test",
                          "flags": "i"
                        }
                      }
                      """;

        RegExpRemoteValue? result = JsonSerializer.Deserialize<RegExpRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.RegExp));
        Assert.That(result.Value.Pattern, Is.EqualTo("test"));
        Assert.That(result.Value.Flags, Is.EqualTo("i"));
    }

    [Test]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": {
                          "pattern": "test",
                          "flags": "i"
                        }
                      }
                      """;

        RegExpRemoteValue? result = JsonSerializer.Deserialize<RegExpRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("regexp"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<RegularExpressionValue>());
        RegularExpressionValue regexValue = (RegularExpressionValue)argumentLocalValue.Value;
        Assert.That(regexValue.Pattern, Is.EqualTo("test"));
        Assert.That(regexValue.Flags, Is.EqualTo("i"));
    }

    [Test]
    public void TestCanConvertToRemoteObjectReference()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": {
                          "pattern": "test",
                          "flags": "i"
                        },
                        "handle": "myHandle",
                        "internalId": "myInternalId"
                      }
                      """;
        
        RegExpRemoteValue? result = JsonSerializer.Deserialize<RegExpRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.That(remoteObjectReference.Handle, Is.EqualTo("myHandle"));
        Assert.That(remoteObjectReference.SharedId, Is.Null);
    }

    [Test]
    public void TestCanUseImplicitConversionToRegularExpressionValue()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": {
                          "pattern": "test",
                          "flags": "i"
                        }
                      }
                      """;

        RegExpRemoteValue? result = JsonSerializer.Deserialize<RegExpRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        RegularExpressionValue regexValue = result;
        Assert.That(regexValue.Pattern, Is.EqualTo("test"));
        Assert.That(regexValue.Flags, Is.EqualTo("i"));
    }

    [Test]
    public void TestDeserializingRegExpRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "regexp"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<RegExpRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingRegExpRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": 2133
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<RegExpRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingRegExpRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-regexp",
                        "value": {
                          "pattern": "test",
                          "flags": "i"
                        }
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<RegExpRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingObjectReferenceRemoteValueWithInvalidHandleTypeThrows()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": {
                          "pattern": "test",
                          "flags": "i"
                        },
                        "handle": 1234,
                        "internalId": "myInternalId"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingObjectReferenceRemoteValueWithInvalidInternalIdTypeThrows()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": {
                          "pattern": "test",
                          "flags": "i"
                        },
                        "handle": "myHandle",
                        "internalId": 2133
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestConvertingToRemoteObjectReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": {
                          "pattern": "test",
                          "flags": "i"
                        }
                      }
                      """;

        RegExpRemoteValue? result = JsonSerializer.Deserialize<RegExpRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.ToRemoteObjectReference(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": {
                          "pattern": "test",
                          "flags": "i"
                        }
                      }
                      """;

        RegExpRemoteValue? result = JsonSerializer.Deserialize<RegExpRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        RegExpRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}
