namespace WebDriverBiDi.Protocol;

public class ConnectionDataReceivedEventArgsTests
{
    [Fact]
    public void TestCanCreateConnectionDataReceivedEventArgs()
    {
        ConnectionDataReceivedEventArgs eventArgs = new(new byte[] { 1 });
        Assert.Equal(1, eventArgs.Data.Length);
        Assert.Equal(1, eventArgs.Data.Span[0]);
        Assert.Empty(eventArgs.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        ConnectionDataReceivedEventArgs eventArgs = new(new byte[] { 1 });
        ConnectionDataReceivedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
