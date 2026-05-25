namespace WebDriverBiDi.Script;

using System.Text.Json;

public class StackTraceTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "callFrames": [
                          {
                            "functionName": "myFunction",
                            "lineNumber": 1,
                            "columnNumber": 5,
                            "url": "http://some.url/file.js"
                          }
                        ]
                      }
                      """;
        StackTrace? stacktrace = JsonSerializer.Deserialize<StackTrace>(json);
        Assert.NotNull(stacktrace);

        Assert.NotNull(stacktrace.CallFrames);
        Assert.Single(stacktrace.CallFrames);
        Assert.IsType<StackFrame>(stacktrace.CallFrames[0]);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "callFrames": [
                          {
                            "functionName": "myFunction",
                            "lineNumber": 1,
                            "columnNumber": 5,
                            "url": "http://some.url/file.js"
                          }
                        ]
                      }
                      """;
        StackTrace? stacktrace = JsonSerializer.Deserialize<StackTrace>(json);
        Assert.NotNull(stacktrace);
        StackTrace copy = stacktrace with { };
        Assert.Equal(stacktrace, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingCallFramesThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StackTrace>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidCallFramesTypeThrows()
    {
        string json = """
                      {
                        "callFrames": {
                          "frame": {
                            "functionName": "myFunction",
                            "lineNumber": 1,
                            "columnNumber": 5,
                            "url": "http://some.url/file.js"
                          }
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StackTrace>(json));
    }
}
