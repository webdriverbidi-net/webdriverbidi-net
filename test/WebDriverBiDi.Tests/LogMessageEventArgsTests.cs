namespace WebDriverBiDi;

[TestFixture]
public class LogMessageEventArgsTests
{
    [Test]
    public void TestCanCreateConnectionDataReceivedEventArgs()
    {
        DateTime testTime = DateTime.Now;
        LogMessageEventArgs eventArgs = new("log message", WebDriverBiDiLogLevel.Info, "test component");
        Assert.That(eventArgs.Message, Is.EqualTo("log message"));
        Assert.That(eventArgs.Level, Is.EqualTo(WebDriverBiDiLogLevel.Info));
        Assert.That(eventArgs.ComponentName, Is.EqualTo("test component"));
        Assert.That(eventArgs.Timestamp, Is.GreaterThanOrEqualTo(testTime));
        Assert.That(eventArgs.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        LogMessageEventArgs eventArgs = new("log message", WebDriverBiDiLogLevel.Info, "test component");
        LogMessageEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));        
    }
}
