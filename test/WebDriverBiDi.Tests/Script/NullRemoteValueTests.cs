namespace WebDriverBiDi.Script;

using System.Text.Json;

public class NullRemoteValueTests
{
    [Fact]
    public void TestCanDeserializeNullRemoteValue()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;

        NullRemoteValue? result = JsonSerializer.Deserialize<NullRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Null, result.Type);
    }

    [Fact]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;

        NullRemoteValue? result = JsonSerializer.Deserialize<NullRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("null", argumentLocalValue.Type);
        Assert.Null(argumentLocalValue.Value);
    }

    [Fact]
    public void TestDeserializingStringRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-null"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NullRemoteValue>(json));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;

        NullRemoteValue? result = JsonSerializer.Deserialize<NullRemoteValue>(json);
        Assert.NotNull(result);
        NullRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
