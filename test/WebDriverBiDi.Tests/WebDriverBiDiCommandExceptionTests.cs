namespace WebDriverBiDi;

public class WebDriverBiDiCommandExceptionTests
{
    [Fact]
    public void TestCanCreateWithNoArguments()
    {
        WebDriverBiDiCommandException exception = new();

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
        ErrorResult errorResult = ErrorResult.FromErrorInformation("invalid argument", "This is a test error message", "remote stack trace");
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult);

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
        ErrorResult errorResult = ErrorResult.FromErrorInformation("unknown command", "This is a test error message", null);
        InvalidOperationException innerException = new("inner exception message");
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult, innerException);

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
        ErrorResult errorResult = ErrorResult.FromErrorInformation("no such frame", "Frame not found", null);
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult);
        Assert.IsType<WebDriverBiDiErrorResponseException>(exception, exactMatch: false);
    }

    [Fact]
    public void TestIsWebDriverBiDiException()
    {
        ErrorResult errorResult = ErrorResult.FromErrorInformation("no such frame", "Frame not found", null);
        WebDriverBiDiCommandException exception = new("Test exception message", errorResult);
        Assert.IsType<WebDriverBiDiException>(exception, exactMatch: false);
    }
}
