namespace WebDriverBiDi;

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
        InvalidOperationException innerException = new("inner exception message");
        WebDriverBiDiConnectionException exception = new("Test exception message", innerException);

        Assert.Equal("Test exception message", exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void TestIsWebDriverBiDiException()
    {
        WebDriverBiDiConnectionException exception = new("Test exception message");
        Assert.IsType<WebDriverBiDiException>(exception, exactMatch: false);
    }

}
