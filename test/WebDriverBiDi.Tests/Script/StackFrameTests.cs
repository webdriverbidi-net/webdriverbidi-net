namespace WebDriverBiDi.Script;

using System.Text.Json;

public class StackFrameTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "functionName": "myFunction",
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "url": "http://some.url/file.js"
                      }
                      """;
        StackFrame? stackFrame = JsonSerializer.Deserialize<StackFrame>(json);
        Assert.NotNull(stackFrame);

        Assert.Equal("myFunction", stackFrame.FunctionName);
        Assert.Equal(1, stackFrame.LineNumber);
        Assert.Equal(5, stackFrame.ColumnNumber);
        Assert.Equal("http://some.url/file.js", stackFrame.Url);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "functionName": "myFunction",
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "url": "http://some.url/file.js"
                      }
                      """;
        StackFrame? stackFrame = JsonSerializer.Deserialize<StackFrame>(json);
        Assert.NotNull(stackFrame);
        StackFrame copy = stackFrame with { };
        Assert.Equal(stackFrame, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingFunctionNameThrows()
    {
        string json = """
                      {
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "url": "http://some.url/file.js"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StackFrame>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidFunctionNameTypeThrows()
    {
        string json = """
                      {
                        "functionName": {},
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "url": "http://some.url/file.js"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StackFrame>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingLineNumberThrows()
    {
        string json = """
                      {
                        "functionName": "myFunction",
                        "columnNumber": 5,
                        "url": "http://some.url/file.js"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StackFrame>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidLineNumberTypeThrows()
    {
        string json = """
                      {
                        "functionName": "myFunction",
                        "lineNumber": {},
                        "columnNumber": 5,
                        "url": "http://some.url/file.js"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StackFrame>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingColumnNumberThrows()
    {
        string json = """
                      {
                        "functionName": "myFunction",
                        "lineNumber": 1,
                        "url": "http://some.url/file.js"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StackFrame>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidColumnNumberTypeThrows()
    {
        string json = """
                      {
                        "functionName": "myFunction",
                        "lineNumber": 1,
                        "columnNumber": {},
                        "url": "http://some.url/file.js"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StackFrame>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingUrlThrows()
    {
        string json = """
                      {
                        "functionName": "myFunction",
                        "lineNumber": 1,
                        "columnNumber": 5
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StackFrame>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidUrlTypeThrows()
    {
        string json = """
                      {
                        "functionName": "myFunction",
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "url": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StackFrame>(json));
    }
}
