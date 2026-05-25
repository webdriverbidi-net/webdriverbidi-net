namespace WebDriverBiDi.Network;

using System.Text.Json;

public class InitiatorTests
{
    [Fact]
    public void TestCanDeserializeInitiator()
    {
        string json = """
                      {
                      }
                      """;
        Initiator? initiator = JsonSerializer.Deserialize<Initiator>(json);
        Assert.NotNull(initiator);

        Assert.Null(initiator.Type);
        Assert.Null(initiator.ColumnNumber);
        Assert.Null(initiator.LineNumber);
        Assert.Null(initiator.StackTrace);
        Assert.Null(initiator.RequestId);
    }

    [Fact]
    public void TestCanDeserializeInitiatorWithOptionalValues()
    {
        string json = """
                      {
                        "type": "script",
                        "lineNumber": 2,
                        "columnNumber": 1,
                        "stackTrace": {
                          "callFrames": [
                            {
                              "functionName": "myFunction",
                              "lineNumber": 2,
                              "columnNumber": 1,
                              "url": "http://some.url/file.js" 
                            }
                          ] 
                        },
                        "request": "myRequestId"
                      }
                      """;
        Initiator? initiator = JsonSerializer.Deserialize<Initiator>(json);
        Assert.NotNull(initiator);

        Assert.Equal(InitiatorType.Script, initiator.Type);
        Assert.Equal(1u, initiator.ColumnNumber);
        Assert.Equal(2u, initiator.LineNumber);
        Assert.NotNull(initiator.StackTrace);
        Assert.Single(initiator.StackTrace.CallFrames);
        Assert.Equal("myRequestId", initiator.RequestId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                      }
                      """;
        Initiator? initiator = JsonSerializer.Deserialize<Initiator>(json);
        Assert.NotNull(initiator);
        Initiator copy = initiator with { };
        Assert.Equal(initiator, copy);
    }

    [Fact]
    public void TestDeserializingInitiatorWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "invalid"
                      }
                      """;
        Assert.Contains("value 'invalid' is not valid for enum type", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Initiator>(json)).Message);
    }
}
