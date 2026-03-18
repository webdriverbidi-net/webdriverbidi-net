namespace WebDriverBiDi;

[TestFixture]
public class WebDriverBiDiCommandExceptionTests
{
    [Test]
    public void TestCanCreateWithNoArguments()
    {
        WebDriverBiDiCommandException exception = new();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.Not.Null);
            Assert.That(exception.ErrorDetails, Is.Not.Null);
            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCode.UnsetErrorCode));
            Assert.That(exception.ProtocolErrorMessage, Is.EqualTo(string.Empty));
            Assert.That(exception.RemoteStackTrace, Is.Null);
            Assert.That(exception.InnerException, Is.Null);
        }
    }

    [Test]
    public void TestCanCreateWithErrorResult()
    {
        ErrorResult errorResult = ErrorResult.FromErrorInformation("invalid argument", "This is a test error message", "remote stack trace");
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.EqualTo("Test exception message"));
            Assert.That(exception.ErrorDetails, Is.SameAs(errorResult));
            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCode.InvalidArgument));
            Assert.That(exception.ProtocolErrorMessage, Is.EqualTo("This is a test error message"));
            Assert.That(exception.RemoteStackTrace, Is.EqualTo("remote stack trace"));
            Assert.That(exception.InnerException, Is.Null);
        }
    }

    [Test]
    public void TestCanCreateWithInnerException()
    {
        ErrorResult errorResult = ErrorResult.FromErrorInformation("unknown command", "This is a test error message", null);
        InvalidOperationException innerException = new("inner exception message");
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult, innerException);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.EqualTo("Test exception message"));
            Assert.That(exception.ErrorDetails, Is.SameAs(errorResult));
            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCode.UnknownCommand));
            Assert.That(exception.ProtocolErrorMessage, Is.EqualTo("This is a test error message"));
            Assert.That(exception.RemoteStackTrace, Is.Null);
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        }
    }

    [Test]
    public void TestIsWebDriverBiDiErrorResponseException()
    {
        ErrorResult errorResult = ErrorResult.FromErrorInformation("no such frame", "Frame not found", null);
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult);
        Assert.That(exception, Is.InstanceOf<WebDriverBiDiErrorResponseException>());
    }

    [Test]
    public void TestIsWebDriverBiDiException()
    {
        ErrorResult errorResult = ErrorResult.FromErrorInformation("no such frame", "Frame not found", null);
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult);
        Assert.That(exception, Is.InstanceOf<WebDriverBiDiException>());
    }
}
