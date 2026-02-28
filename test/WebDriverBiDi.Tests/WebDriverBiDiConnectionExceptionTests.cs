namespace WebDriverBiDi;

using System.Text.Json;
using WebDriverBiDi.Protocol;

[TestFixture]
public class WebDriverBiDiConnectionExceptionTests
{
    [Test]
    public void TestCanCreate()
    {
        WebDriverBiDiConnectionException exception = new("Test exception message");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.EqualTo("Test exception message"));
            Assert.That(exception.InnerException, Is.Null);
        }
    }

    [Test]
    public void TestCanCreateWithInnerException()
    {
        ErrorResult errorResult = CreateErrorResult("unknown command", "This is a test error message", null);
        InvalidOperationException innerException = new("inner exception message");
        WebDriverBiDiConnectionException exception = new("Test exception message", innerException);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.EqualTo("Test exception message"));
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        }
    }

    [Test]
    public void TestIsWebDriverBiDiException()
    {
        ErrorResult errorResult = CreateErrorResult("no such frame", "Frame not found", null);
        WebDriverBiDiConnectionException exception = new("Test exception message");
        Assert.That(exception, Is.InstanceOf<WebDriverBiDiException>());
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
        ErrorResponseMessage messageResult = JsonSerializer.Deserialize<ErrorResponseMessage>(json)!;
        return messageResult.GetErrorResponseData();
    }
}
