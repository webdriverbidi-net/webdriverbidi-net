namespace WebDriverBiDi.Protocol;

public class ConnectionErrorEventArgsTests
{
    [Fact]
    public void TestCanCreateConnectionErrorEventArgs()
    {
        Exception exception = new("test error");
        ConnectionErrorEventArgs eventArgs = new(exception);
        Assert.Same(exception, eventArgs.Exception);
        Assert.Equal("test error", eventArgs.Exception.Message);
        Assert.Empty(eventArgs.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        ConnectionErrorEventArgs eventArgs = new(new Exception("test"));
        ConnectionErrorEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
