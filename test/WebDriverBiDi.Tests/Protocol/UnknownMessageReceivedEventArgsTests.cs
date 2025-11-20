namespace WebDriverBiDi.Protocol;

[TestFixture]
public class UnknownMessageReceivedEventArgsTests
{
    [Test]
    public void TestCanCreateErrorReceivedEventArgsWithNullErrorData()
    {
        UnknownMessageReceivedEventArgs eventArgs = new("unknown message");
        Assert.That(eventArgs.Message, Is.EqualTo("unknown message"));
        Assert.That(eventArgs.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        UnknownMessageReceivedEventArgs eventArgs = new("unknown message");
        UnknownMessageReceivedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));        
    }
}
