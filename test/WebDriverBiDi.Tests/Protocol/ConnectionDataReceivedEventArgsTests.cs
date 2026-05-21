namespace WebDriverBiDi.Protocol;

public class ConnectionDataReceivedEventArgsTests
{
    [Fact]
    public void TestCanCreateConnectionDataReceivedEventArgs()
    {
        ConnectionDataReceivedEventArgs eventArgs = new([1]);
        Assert.Single(eventArgs.Data);
        Assert.Equal(1, eventArgs.Data[0]);
        Assert.Empty(eventArgs.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        ConnectionDataReceivedEventArgs eventArgs = new([1]);
        ConnectionDataReceivedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
