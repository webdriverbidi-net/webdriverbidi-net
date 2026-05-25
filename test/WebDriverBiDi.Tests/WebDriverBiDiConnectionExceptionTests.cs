namespace WebDriverBiDi;

using System.Text.Json;
using WebDriverBiDi.Protocol;

public class WebDriverBiDiConnectionExceptionTests
{
    [Fact]
    public void TestCanCreateWithNoArguments()
    {
        WebDriverBiDiConnectionException exception = new();

        Assert.NotNull(exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void TestCanCreate()
    {
        WebDriverBiDiConnectionException exception = new("Test exception message");

        Assert.Equal("Test exception message", exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void TestCanCreateWithInnerException()
    {
        ErrorResult errorResult = CreateErrorResult("unknown command", "This is a test error message", null);
        InvalidOperationException innerException = new("inner exception message");
        WebDriverBiDiConnectionException exception = new("Test exception message", innerException);

        Assert.Equal("Test exception message", exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void TestIsWebDriverBiDiException()
    {
        ErrorResult errorResult = CreateErrorResult("no such frame", "Frame not found", null);
        WebDriverBiDiConnectionException exception = new("Test exception message");
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
