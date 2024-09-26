namespace WebDriverBiDi.Protocol;

using System.Text.Json;
using WebDriverBiDi.TestUtilities;

[TestFixture]
public class MessageTests
{
    [Test]
    public void TestCanDeserializeCommandResponseMessage()
    {
        string json = """
                      {
                        "type": "success",
                        "id": 1,
                        "result": {
                          "value": "response value"
                        }
                      }
                      """;
        CommandResponseMessage<TestCommandResult>? result = JsonSerializer.Deserialize<CommandResponseMessage<TestCommandResult>>(json);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Type, Is.EqualTo("success"));
            Assert.That(result.Result, Is.InstanceOf<TestCommandResult>());
            Assert.That(((TestCommandResult)result.Result).Value, Is.EqualTo("response value"));
        });
    }

    [Test]
    public void TestCanDeserializeEventMessage()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        EventMessage<TestEventArgs>? result = JsonSerializer.Deserialize<EventMessage<TestEventArgs>>(json);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Type, Is.EqualTo("event"));
            Assert.That(result.EventData, Is.InstanceOf<TestEventArgs>());
            Assert.That(result.EventName, Is.EqualTo("protocol.event"));
            Assert.That(((TestEventArgs)result.EventData).ParamName, Is.EqualTo("paramValue"));
        });
    }

    [Test]
    public void TestCanDeserializeCommandErrorMessage()
    {
        string json = """
                      {
                        "type": "error",
                        "id": 1,
                        "error": "unknown error",
                        "message": "This is a test error message"
                      }
                      """;
        ErrorResponseMessage? result = JsonSerializer.Deserialize<ErrorResponseMessage>(json);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Type, Is.EqualTo("error"));
            Assert.That(result.CommandId, Is.EqualTo(1));
            Assert.That(result.ErrorType, Is.EqualTo("unknown error"));
            Assert.That(result.ErrorMessage, Is.EqualTo("This is a test error message"));
        });
    }

    [Test]
    public void TestCanDeserializeNonCommandErrorMessage()
    {
        string json = """
                      {
                        "type": "error",
                        "id": null,
                        "error": "unknown error",
                        "message": "This is a test error message"
                      }
                      """;
        ErrorResponseMessage? result = JsonSerializer.Deserialize<ErrorResponseMessage>(json);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Type, Is.EqualTo("error"));
            Assert.That(result.CommandId, Is.Null);
            Assert.That(result.ErrorType, Is.EqualTo("unknown error"));
            Assert.That(result.ErrorMessage, Is.EqualTo("This is a test error message"));
        });
    }
}