namespace WebDriverBiDi;

public class LogMessageEventArgsTests
{
    [Fact]
    public void TestCanCreateConnectionDataReceivedEventArgs()
    {
        DateTime testTime = DateTime.Now;
        LogMessageEventArgs eventArgs = new("log message", WebDriverBiDiLogLevel.Info, "test component");
        Assert.Equal("log message", eventArgs.Message);
        Assert.Equal(WebDriverBiDiLogLevel.Info, eventArgs.Level);
        Assert.Equal("test component", eventArgs.ComponentName);
        Assert.True(eventArgs.Timestamp >= testTime);
        Assert.Empty(eventArgs.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        LogMessageEventArgs eventArgs = new("log message", WebDriverBiDiLogLevel.Info, "test component");
        LogMessageEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
