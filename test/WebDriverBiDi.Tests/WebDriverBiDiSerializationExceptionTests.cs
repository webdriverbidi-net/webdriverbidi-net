namespace WebDriverBiDi;

public class WebDriverBiDiSerializationExceptionTests
{
    [Fact]
    public void TestCanCreateWithNoArguments()
    {
        WebDriverBiDiSerializationException exception = new();

        Assert.NotNull(exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void TestCanCreate()
    {
        WebDriverBiDiSerializationException exception = new("Test exception message");

        Assert.Equal("Test exception message", exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void TestCanCreateWithInnerException()
    {
        InvalidOperationException innerException = new("inner exception message");
        WebDriverBiDiSerializationException exception = new("Test exception message", innerException);

        Assert.Equal("Test exception message", exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void TestIsWebDriverBiDiException()
    {
        WebDriverBiDiSerializationException exception = new("Test exception message");
        Assert.IsType<WebDriverBiDiException>(exception, exactMatch: false);
    }
}
