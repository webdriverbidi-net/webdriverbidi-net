namespace WebDriverBiDi.Script;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class ExceptionDetailsTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
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
        ExceptionDetails? exceptionDetails = JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions);
        Assert.That(exceptionDetails, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exceptionDetails.Text, Is.EqualTo("exception message"));
            Assert.That(exceptionDetails.LineNumber, Is.EqualTo(1));
            Assert.That(exceptionDetails.ColumnNumber, Is.EqualTo(5));
            Assert.That(exceptionDetails.Exception.ValueAs<string>(), Is.EqualTo("myException"));
            Assert.That(exceptionDetails.StackTrace.CallFrames, Is.Empty);
        }
    }

    [Test]
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
        ExceptionDetails? exceptionDetails = JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions);
        Assert.That(exceptionDetails, Is.Not.Null);
        ExceptionDetails copy = exceptionDetails with { };
        Assert.That(copy, Is.EqualTo(exceptionDetails));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<ExceptionDetails>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
