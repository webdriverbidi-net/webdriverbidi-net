namespace WebDriverBiDi.Script;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class StackTraceTests
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
        StackTrace? stacktrace = JsonSerializer.Deserialize<StackTrace>(json, deserializationOptions);
        Assert.That(stacktrace, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(stacktrace.CallFrames, Is.Not.Null);
            Assert.That(stacktrace.CallFrames, Has.Count.EqualTo(1));
            Assert.That(stacktrace.CallFrames[0], Is.TypeOf<StackFrame>());
        });
    }

    [Test]
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
        StackTrace? stacktrace = JsonSerializer.Deserialize<StackTrace>(json, deserializationOptions);
        Assert.That(stacktrace, Is.Not.Null);
        StackTrace copy = stacktrace with { };
        Assert.That(copy, Is.EqualTo(stacktrace));
    }

    [Test]
    public void TestDeserializeWithMissingCallFramesThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<StackTrace>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<StackTrace>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
