namespace WebDriverBiDi;

public class WebDriverBiDiExceptionTests
{
    [Fact]
    public void TestCanCreateWithNoArguments()
    {
        WebDriverBiDiException exception = new();

        Assert.NotNull(exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void TestCanCreate()
    {
        WebDriverBiDiException exception = new("Test exception message");

        Assert.Equal("Test exception message", exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void TestCanCreateWithInnerException()
    {
        InvalidOperationException innerException = new("inner exception message");
        WebDriverBiDiException exception = new("Test exception message", innerException);

        Assert.Equal("Test exception message", exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }
}
