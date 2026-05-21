namespace WebDriverBiDi.Protocol;

using System.Text.Json;
using WebDriverBiDi.TestUtilities;

public class MessageTests
{
    [Fact]
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
        Assert.NotNull(result);

        Assert.Equal("success", result.Type);
        Assert.IsType<TestCommandResult>(result.Result);
        Assert.Equal("response value", ((TestCommandResult)result.Result).Value);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
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
        Assert.NotNull(result);

        Assert.Equal("event", result.Type);
        Assert.IsType<TestEventArgs>(result.EventData);
        Assert.Equal("protocol.event", result.EventName);
        Assert.Equal("paramValue", ((TestEventArgs)result.EventData).ParamName);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
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
        Assert.NotNull(result);

        Assert.Equal("error", result.Type);
        Assert.Equal(1, result.CommandId);
        Assert.Equal("unknown error", result.ErrorType);
        Assert.Equal(ErrorCode.UnknownError, result.ErrorCode);
        Assert.Equal("This is a test error message", result.ErrorMessage);
        Assert.Empty(result.AdditionalData);
        Assert.Null(result.StackTrace);
    }

    [Fact]
    public void TestCanDeserializeCommandErrorMessageWithUnknownErrorCode()
    {
        string json = """
                      {
                        "type": "error",
                        "id": 1,
                        "error": "invalid error code",
                        "message": "This is a test error message"
                      }
                      """;
        ErrorResponseMessage? result = JsonSerializer.Deserialize<ErrorResponseMessage>(json);
        Assert.NotNull(result);

        Assert.Equal("error", result.Type);
        Assert.Equal(1, result.CommandId);
        Assert.Equal("invalid error code", result.ErrorType);
        Assert.Equal(ErrorCode.UnsetErrorCode, result.ErrorCode);
        Assert.Equal("This is a test error message", result.ErrorMessage);
        Assert.Empty(result.AdditionalData);
        Assert.Null(result.StackTrace);
    }

    [Fact]
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
        Assert.NotNull(result);

        Assert.Equal("error", result.Type);
        Assert.Null(result.CommandId);
        Assert.Equal("unknown error", result.ErrorType);
        Assert.Equal(ErrorCode.UnknownError, result.ErrorCode);
        Assert.Equal("This is a test error message", result.ErrorMessage);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCanDeserializeErrorMessageWithStackTrace()
    {
        string json = """
                      {
                        "type": "error",
                        "id": 1,
                        "error": "unknown error",
                        "message": "This is a test error message",
                        "stacktrace": "full stack trace"
                      }
                      """;
        ErrorResponseMessage? result = JsonSerializer.Deserialize<ErrorResponseMessage>(json);
        Assert.NotNull(result);

        Assert.Equal("error", result.Type);
        Assert.Equal(1, result.CommandId);
        Assert.Equal("unknown error", result.ErrorType);
        Assert.Equal(ErrorCode.UnknownError, result.ErrorCode);
        Assert.Equal("This is a test error message", result.ErrorMessage);
        Assert.Empty(result.AdditionalData);
        Assert.Equal("full stack trace", result.StackTrace);
    }

    [Fact]
    public void TestCanGetErrorResultFromMessage()
    {
        string json = """
                      {
                        "type": "error",
                        "id": 1,
                        "error": "unknown error",
                        "message": "This is a test error message",
                        "stacktrace": "full stack trace"
                      }
                      """;
        ErrorResponseMessage? messageResult = JsonSerializer.Deserialize<ErrorResponseMessage>(json);
        Assert.NotNull(messageResult);
        ErrorResult result = messageResult.GetErrorResponseData();

        Assert.True(result.IsError);
        Assert.Equal("unknown error", result.ErrorType);
        Assert.Equal(ErrorCode.UnknownError, result.ErrorCode);
        Assert.Equal("This is a test error message", result.ErrorMessage);
        Assert.Empty(result.AdditionalData);
        Assert.Equal("full stack trace", result.StackTrace);
    }

    [Fact]
    public void TestCanDeserializeMessageWithAdditionalData()
    {
        string json = """
                      {
                        "type": "success",
                        "id": 1,
                        "result": {
                          "value": "response value"
                        },
                        "additionalProperty": "additional value"
                      }
                      """;
        CommandResponseMessage<TestCommandResult>? result = JsonSerializer.Deserialize<CommandResponseMessage<TestCommandResult>>(json);
        Assert.NotNull(result);

        Assert.Equal("success", result.Type);
        Assert.IsType<TestCommandResult>(result.Result);
        Assert.Equal("response value", ((TestCommandResult)result.Result).Value);
        Assert.Single(result.AdditionalData);
    }

    [Fact]
    public void TestCommandResponseMessageThrowsWhenResultIsNull()
    {
        CommandResponseMessage<TestCommandResult> message = new();
        Assert.Equal("Result cannot be null", Assert.ThrowsAny<InvalidOperationException>(() => _ = message.Result).Message);
    }

    [Fact]
    public void TestAdditionalDataReturnsCachedInstanceOnRepeatedAccess()
    {
        string json = """
                      {
                        "type": "success",
                        "id": 1,
                        "result": {
                          "value": "response value"
                        },
                        "additionalProperty": "additional value"
                      }
                      """;
        CommandResponseMessage<TestCommandResult>? result = JsonSerializer.Deserialize<CommandResponseMessage<TestCommandResult>>(json);
        Assert.NotNull(result);
        ReceivedDataDictionary first = result.AdditionalData;
        ReceivedDataDictionary second = result.AdditionalData;
        Assert.Same(second, first);
    }

    [Fact]
    public void TestErrorCodeReturnsCachedValueOnRepeatedAccess()
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
        Assert.NotNull(result);
        ErrorCode first = result.ErrorCode;
        ErrorCode second = result.ErrorCode;
        Assert.Equal(ErrorCode.UnknownError, first);
        Assert.Equal(first, second);
    }
}
