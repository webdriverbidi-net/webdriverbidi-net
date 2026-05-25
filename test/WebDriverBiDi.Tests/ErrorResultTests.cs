namespace WebDriverBiDi;

using System.Text.Json;
using WebDriverBiDi.Protocol;

public class ErrorResultTests
{
    [Fact]
    public void TestCanCreateErrorResult()
    {
        // ErrorResult constructor is internal, so we will create it by
        // creating an ErrorResponseMessage and calling GetErrorResponseData
        // for it.
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
    public void TestCopySemantics()
    {
        // ErrorResult constructor is internal, so we will create it by
        // creating an ErrorResponseMessage and calling GetErrorResponseData
        // for it.
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
        ErrorResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
