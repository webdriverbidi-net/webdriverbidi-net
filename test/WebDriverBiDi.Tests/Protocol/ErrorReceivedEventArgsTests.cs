namespace WebDriverBiDi.Protocol;

using System.Text.Json;

public class ErrorReceivedEventArgsTests
{
    [Fact]
    public void TestCanCreateErrorReceivedEventArgs()
    {
        string json = """
                      {
                        "id": 123,
                        "type": "error",
                        "error": "my error code",
                        "message": "error message",
                        "stacktrace": "stack trace"
                      }
                      """;
        ErrorResponseMessage? errorMessage = JsonSerializer.Deserialize<ErrorResponseMessage>(json);
        Assert.NotNull(errorMessage);
        ErrorReceivedEventArgs eventArgs = new(errorMessage.GetErrorResponseData());

        Assert.NotNull(eventArgs.ErrorData);
        Assert.True(eventArgs.ErrorData.IsError);
        Assert.Equal(ErrorCode.UnsetErrorCode, eventArgs.ErrorData.ErrorCode);
        Assert.Equal("my error code", eventArgs.ErrorData.ErrorType);
        Assert.Equal("error message", eventArgs.ErrorData.ErrorMessage);
        Assert.Equal("stack trace", eventArgs.ErrorData.StackTrace);
    }

    [Fact]
    public void TestCreateErrorReceivedEventArgsWithNullErrorDataThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new ErrorReceivedEventArgs(null!));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "id": 123,
                        "type": "error",
                        "error": "my error code",
                        "message": "error message",
                        "stacktrace": "stack trace"
                      }
                      """;
        ErrorResponseMessage? errorMessage = JsonSerializer.Deserialize<ErrorResponseMessage>(json);
        Assert.NotNull(errorMessage);
        ErrorReceivedEventArgs eventArgs = new(errorMessage.GetErrorResponseData());
        ErrorReceivedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
