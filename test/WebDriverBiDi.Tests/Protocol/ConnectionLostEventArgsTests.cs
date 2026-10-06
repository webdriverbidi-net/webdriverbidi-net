namespace WebDriverBiDi.Protocol;

public class ConnectionLostEventArgsTests
{
    [Fact]
    public void TestCanCreateConnectionLostEventArgs()
    {
        WebDriverBiDiConnectionException exception = new("Remote end closed the connection");
        ConnectionLostEventArgs eventArgs = new(exception);
        Assert.Same(exception, eventArgs.Exception);
        Assert.Empty(eventArgs.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        ConnectionLostEventArgs eventArgs = new(new WebDriverBiDiConnectionException("test"));
        ConnectionLostEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
