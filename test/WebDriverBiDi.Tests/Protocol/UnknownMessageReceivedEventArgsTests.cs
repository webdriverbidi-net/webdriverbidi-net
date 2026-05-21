namespace WebDriverBiDi.Protocol;

public class UnknownMessageReceivedEventArgsTests
{
    [Fact]
    public void TestCanCreateErrorReceivedEventArgsWithNullErrorData()
    {
        UnknownMessageReceivedEventArgs eventArgs = new("unknown message");
        Assert.Equal("unknown message", eventArgs.Message);
        Assert.Empty(eventArgs.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        UnknownMessageReceivedEventArgs eventArgs = new("unknown message");
        UnknownMessageReceivedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
