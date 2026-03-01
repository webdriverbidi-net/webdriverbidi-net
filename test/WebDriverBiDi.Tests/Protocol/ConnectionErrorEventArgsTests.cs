namespace WebDriverBiDi.Protocol;

[TestFixture]
public class ConnectionErrorEventArgsTests
{
    [Test]
    public void TestCanCreateConnectionErrorEventArgs()
    {
        Exception exception = new("test error");
        ConnectionErrorEventArgs eventArgs = new(exception);
        Assert.That(eventArgs.Exception, Is.SameAs(exception));
        Assert.That(eventArgs.Exception.Message, Is.EqualTo("test error"));
        Assert.That(eventArgs.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ConnectionErrorEventArgs eventArgs = new(new Exception("test"));
        ConnectionErrorEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }
}
