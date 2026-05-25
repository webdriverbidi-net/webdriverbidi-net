namespace WebDriverBiDi.Script;

using System.Text.Json;

public class RegExpRemoteValueTests
{
    [Fact]
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

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.RegExp, result.Type);
        Assert.Equal("test", result.Value.Pattern);
        Assert.Equal("i", result.Value.Flags);
    }

    [Fact]
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

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("regexp", argumentLocalValue.Type);
        Assert.NotNull(argumentLocalValue.Value);
        Assert.IsType<RegularExpressionValue>(argumentLocalValue.Value);
        RegularExpressionValue regexValue = (RegularExpressionValue)argumentLocalValue.Value;
        Assert.Equal("test", regexValue.Pattern);
        Assert.Equal("i", regexValue.Flags);
    }

    [Fact]
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

        Assert.NotNull(result);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.Equal("myHandle", remoteObjectReference.Handle);
        Assert.Null(remoteObjectReference.SharedId);
    }

    [Fact]
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

        Assert.NotNull(result);
        RegularExpressionValue regexValue = result;
        Assert.Equal("test", regexValue.Pattern);
        Assert.Equal("i", regexValue.Flags);
    }

    [Fact]
    public void TestDeserializingRegExpRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "regexp"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RegExpRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingRegExpRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RegExpRemoteValue>(json));
    }

    [Fact]
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

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RegExpRemoteValue>(json));
    }

    [Fact]
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

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json));
    }

    [Fact]
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

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json));
    }

    [Fact]
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

        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.ToRemoteObjectReference());
    }

    [Fact]
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
        Assert.NotNull(result);
        RegExpRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
