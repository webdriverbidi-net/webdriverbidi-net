namespace WebDriverBiDi;

using System.Text.Json;
using WebDriverBiDi.Protocol;

[TestFixture]
public class WebDriverBiDiCommandExceptionTests
{
    [Test]
    public void TestCanCreateWithErrorResult()
    {
        ErrorResult errorResult = CreateErrorResult("invalid argument", "This is a test error message", "remote stack trace");
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.EqualTo("Test exception message"));
            Assert.That(exception.ErrorResult, Is.SameAs(errorResult));
            Assert.That(exception.ErrorType, Is.EqualTo("invalid argument"));
            Assert.That(exception.ProtocolErrorMessage, Is.EqualTo("This is a test error message"));
            Assert.That(exception.RemoteStackTrace, Is.EqualTo("remote stack trace"));
            Assert.That(exception.InnerException, Is.Null);
        }
    }

    [Test]
    public void TestCanCreateWithInnerException()
    {
        ErrorResult errorResult = CreateErrorResult("unknown command", "This is a test error message", null);
        InvalidOperationException innerException = new("inner exception message");
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult, innerException);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.EqualTo("Test exception message"));
            Assert.That(exception.ErrorResult, Is.SameAs(errorResult));
            Assert.That(exception.ErrorType, Is.EqualTo("unknown command"));
            Assert.That(exception.ProtocolErrorMessage, Is.EqualTo("This is a test error message"));
            Assert.That(exception.RemoteStackTrace, Is.Null);
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        }
    }

    [Test]
    public void TestIsWebDriverBiDiErrorResponseException()
    {
        ErrorResult errorResult = CreateErrorResult("no such frame", "Frame not found", null);
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult);
        Assert.That(exception, Is.InstanceOf<WebDriverBiDiErrorResponseException>());
    }

    [Test]
    public void TestIsWebDriverBiDiException()
    {
        ErrorResult errorResult = CreateErrorResult("no such frame", "Frame not found", null);
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult);
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
