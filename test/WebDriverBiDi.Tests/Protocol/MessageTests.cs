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
    public void TestCanDeserializeMessageWithComplexAdditionalData()
    {
        // The JSON here is carefully crafted to hit all of the permutations of
        // JSON values that can be deserialized into a ReceivedDataDictionary,
        // including string, number (integer and floating point), boolean (true
        // and false), null, array-like lists (JSON arrays), and maps (including
        // JSON objects). If the implementation changes and/or code coverage drops
        // for the converter in JsonConverterUtilities, this test will have to be
        // updated.
        string json = """
                      {
                        "type": "success",
                        "id": 1,
                        "result": {
                          "value": "response value"
                        },
                        "stringProperty": "stringValue",
                        "intValue": 123,
                        "floatValue": 456.78,
                        "trueBoolValue": true,
                        "falseBoolValue": false,
                        "nullValue": null,
                        "listValue": [
                          "listString",
                          901,
                          true,
                          null
                        ],
                        "objectValue": {
                          "objectProperty": "objectValue"
                        }
                      }
                      """;      
        CommandResponseMessage<TestCommandResult>? result = JsonSerializer.Deserialize<CommandResponseMessage<TestCommandResult>>(json);
        Assert.NotNull(result);

        Assert.Equal("success", result.Type);
        Assert.IsType<TestCommandResult>(result.Result);
        Assert.Equal("response value", ((TestCommandResult)result.Result).Value);
        Assert.Equal(8, result.AdditionalData.Count);

        Assert.True(result.AdditionalData.ContainsKey("stringProperty"));
        object? additionalDataValue = result.AdditionalData["stringProperty"];
        Assert.NotNull(additionalDataValue);
        Assert.IsType<string>(additionalDataValue);
        Assert.Equal("stringValue", additionalDataValue);

        Assert.True(result.AdditionalData.ContainsKey("intValue"));
        additionalDataValue = result.AdditionalData["intValue"];
        Assert.NotNull(additionalDataValue);
        Assert.IsType<long>(additionalDataValue);
        Assert.Equal(123L, additionalDataValue);

        Assert.True(result.AdditionalData.ContainsKey("floatValue"));
        additionalDataValue = result.AdditionalData["floatValue"];
        Assert.NotNull(additionalDataValue);
        Assert.IsType<double>(additionalDataValue);
        Assert.Equal(456.78, additionalDataValue);

        Assert.True(result.AdditionalData.ContainsKey("trueBoolValue"));
        additionalDataValue = result.AdditionalData["trueBoolValue"];
        Assert.NotNull(additionalDataValue);
        Assert.IsType<bool>(additionalDataValue);
        Assert.True((bool)additionalDataValue);

        Assert.True(result.AdditionalData.ContainsKey("falseBoolValue"));
        additionalDataValue = result.AdditionalData["falseBoolValue"];
        Assert.NotNull(additionalDataValue);
        Assert.IsType<bool>(additionalDataValue);
        Assert.False((bool)additionalDataValue);

        Assert.True(result.AdditionalData.ContainsKey("nullValue"));
        additionalDataValue = result.AdditionalData["nullValue"];
        Assert.Null(additionalDataValue);

        Assert.True(result.AdditionalData.ContainsKey("listValue"));
        additionalDataValue = result.AdditionalData["listValue"];
        Assert.NotNull(additionalDataValue);
        Assert.IsType<ReceivedDataList>(additionalDataValue);
        ReceivedDataList receivedDataList = (ReceivedDataList)additionalDataValue;
        Assert.Equal(4, receivedDataList.Count);

        Assert.True(result.AdditionalData.ContainsKey("objectValue"));
        additionalDataValue = result.AdditionalData["objectValue"];
        Assert.NotNull(additionalDataValue);
        Assert.IsType<ReceivedDataDictionary>(additionalDataValue);
        ReceivedDataDictionary receivedDataDictionary = (ReceivedDataDictionary)additionalDataValue;
        Assert.Single(receivedDataDictionary);
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
