namespace WebDriverBiDi.Script;

using System.Text.Json;

public class ExceptionDetailsTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "text": "exception message",
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "exception": {
                          "type": "string",
                          "value": "myException"
                        },
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        ExceptionDetails? exceptionDetails = JsonSerializer.Deserialize<ExceptionDetails>(json);
        Assert.NotNull(exceptionDetails);

        Assert.Equal("exception message", exceptionDetails.Text);
        Assert.Equal(1, exceptionDetails.LineNumber);
        Assert.Equal(5, exceptionDetails.ColumnNumber);
        Assert.Equal("myException", exceptionDetails.Exception.ConvertTo<StringRemoteValue>().Value);
        Assert.Empty(exceptionDetails.StackTrace.CallFrames);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "text": "exception message",
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "exception": {
                          "type": "string",
                          "value": "myException"
                        },
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        ExceptionDetails? exceptionDetails = JsonSerializer.Deserialize<ExceptionDetails>(json);
        Assert.NotNull(exceptionDetails);
        ExceptionDetails copy = exceptionDetails with { };
        Assert.Equal(exceptionDetails, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingTextThrows()
    {
        string json = """
                      {
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "exception": {
                          "type": "string",
                          "value": "myException"
                        },
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ExceptionDetails>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidTextTypeThrows()
    {
        string json = """
                      {
                        "text": true,
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "exception": {
                          "type": "string",
                          "value": "myException"
                        },
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ExceptionDetails>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingLineNumberThrows()
    {
        string json = """
                      {
                        "text": "exception message",
                        "columnNumber": 5,
                        "exception": {
                          "type": "string",
                          "value": "myException"
                        },
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ExceptionDetails>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidLineNumberTypeThrows()
    {
        string json = """
                      {
                        "text": "exception message",
                        "lineNumber": true,
                        "columnNumber": 5,
                        "exception": {
                          "type": "string",
                          "value": "myException"
                        },
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ExceptionDetails>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingColumnNumberThrows()
    {
        string json = """
                      {
                        "text": "exception message",
                        "lineNumber": 1,
                        "exception": {
                          "type": "string",
                          "value": "myException"
                        },
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ExceptionDetails>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidColumnNumberTypeThrows()
    {
        string json = """
                      {
                        "text": "exception message",
                        "lineNumber": 1,
                        "columnNumber": true,
                        "exception": {
                          "type": "string",
                          "value": "myException"
                        },
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ExceptionDetails>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingExceptionThrows()
    {
        string json = """
                      {
                        "text": "exception message",
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ExceptionDetails>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidExceptionTypeThrows()
    {
        string json = """
                      {
                        "text": "exception message",
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "exception": "myException",
                        "stackTrace": {
                          "callFrames": []
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ExceptionDetails>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingStackTraceThrows()
    {
        string json = """
                      {
                        "text": "exception message",
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "exception": {
                          "type": "string",
                          "value": "myException"
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ExceptionDetails>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidStackTraceTypeThrows()
    {
        string json = """
                      {
                        "text": "exception message",
                        "lineNumber": 1,
                        "columnNumber": 5,
                        "exception": {
                          "type": "string",
                          "value": "myException"
                        },
                        "stackTrace": "stacktrace"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ExceptionDetails>(json));
    }
}
