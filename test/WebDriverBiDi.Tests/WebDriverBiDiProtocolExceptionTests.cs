namespace WebDriverBiDi;

using System.Text.Json;
using WebDriverBiDi.Protocol;

public class WebDriverBiDiProtocolExceptionTests
{
    [Fact]
    public void TestCanCreateWithNoArguments()
    {
        WebDriverBiDiProtocolException exception = new();

        Assert.NotNull(exception.Message);
        Assert.NotNull(exception.ErrorDetails);
        Assert.Equal(ErrorCode.UnsetErrorCode, exception.ErrorCode);
        Assert.Equal(string.Empty, exception.ProtocolErrorMessage);
        Assert.Null(exception.RemoteStackTrace);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void TestCanCreateWithErrorResult()
    {
        ErrorResult errorResult = CreateErrorResult("invalid argument", "This is a test error message", "remote stack trace");
        WebDriverBiDiProtocolException exception = new("Test exception message", errorResult);

        Assert.Equal("Test exception message", exception.Message);
        Assert.Same(errorResult, exception.ErrorDetails);
        Assert.Equal(ErrorCode.InvalidArgument, exception.ErrorCode);
        Assert.Equal("This is a test error message", exception.ProtocolErrorMessage);
        Assert.Equal("remote stack trace", exception.RemoteStackTrace);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void TestCanCreateWithInnerException()
    {
        ErrorResult errorResult = CreateErrorResult("unknown command", "This is a test error message", null);
        InvalidOperationException innerException = new("inner exception message");
        WebDriverBiDiProtocolException exception = new("Test exception message", errorResult, innerException);

        Assert.Equal("Test exception message", exception.Message);
        Assert.Same(errorResult, exception.ErrorDetails);
        Assert.Equal(ErrorCode.UnknownCommand, exception.ErrorCode);
        Assert.Equal("This is a test error message", exception.ProtocolErrorMessage);
        Assert.Null(exception.RemoteStackTrace);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void TestIsWebDriverBiDiErrorResponseException()
    {
        ErrorResult errorResult = CreateErrorResult("no such frame", "Frame not found", null);
        WebDriverBiDiProtocolException exception = new("Test exception message", errorResult);
        Assert.IsType<WebDriverBiDiErrorResponseException>(exception, exactMatch: false);
    }

    [Fact]
    public void TestIsWebDriverBiDiException()
    {
        ErrorResult errorResult = CreateErrorResult("no such frame", "Frame not found", null);
        WebDriverBiDiProtocolException exception = new("Test exception message", errorResult);
        Assert.IsType<WebDriverBiDiException>(exception, exactMatch: false);
    }

    private static ErrorResult CreateErrorResult(string errorType, string errorMessage, string? stackTrace)
    {
        string stackTraceProperty = stackTrace is not null
            ? $""","stacktrace": "{stackTrace}" """
            : string.Empty;
        string json = $$"""
                        {
                          "type": "error",
                          "id": 1,
                          "error": "{{errorType}}",
                          "message": "{{errorMessage}}"{{stackTraceProperty}}
                        }
                        """;
        ErrorResponseMessage? messageResult = JsonSerializer.Deserialize<ErrorResponseMessage>(json);
        Assert.NotNull(messageResult);
        return messageResult.GetErrorResponseData();
    }
}
