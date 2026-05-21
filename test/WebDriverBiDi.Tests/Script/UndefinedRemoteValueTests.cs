namespace WebDriverBiDi.Script;

using System.Text.Json;

public class UndefinedRemoteValueTests
{
    [Fact]
    public void TestCanDeserializeUndefinedRemoteValue()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;

        UndefinedRemoteValue? result = JsonSerializer.Deserialize<UndefinedRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Undefined, result.Type);
    }

    [Fact]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;

        UndefinedRemoteValue? result = JsonSerializer.Deserialize<UndefinedRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("undefined", argumentLocalValue.Type);
        Assert.Null(argumentLocalValue.Value);
    }

    [Fact]
    public void TestDeserializingStringRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-undefined"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UndefinedRemoteValue>(json));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;

        UndefinedRemoteValue? result = JsonSerializer.Deserialize<UndefinedRemoteValue>(json);
        Assert.NotNull(result);
        UndefinedRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
