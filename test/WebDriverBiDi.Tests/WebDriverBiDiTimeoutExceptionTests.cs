namespace WebDriverBiDi;

public class WebDriverBiDiTimeoutExceptionTests
{
    [Fact]
    public void TestCanCreateWithNoArguments()
    {
        WebDriverBiDiTimeoutException exception = new();

        Assert.NotNull(exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void TestCanCreate()
    {
        WebDriverBiDiTimeoutException exception = new("Test exception message");

        Assert.Equal("Test exception message", exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void TestCanCreateWithInnerException()
    {
        InvalidOperationException innerException = new("inner exception message");
        WebDriverBiDiTimeoutException exception = new("Test exception message", innerException);

        Assert.Equal("Test exception message", exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void TestIsWebDriverBiDiException()
    {
        WebDriverBiDiTimeoutException exception = new("Test exception message");
        Assert.IsType<WebDriverBiDiException>(exception, exactMatch: false);
    }
}
