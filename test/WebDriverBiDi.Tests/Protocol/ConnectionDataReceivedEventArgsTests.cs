namespace WebDriverBiDi.Protocol;

[TestFixture]
public class ConnectionDataReceivedEventArgsTests
{
    [Test]
    public void TestCanCreateConnectionDataReceivedEventArgs()
    {
        ConnectionDataReceivedEventArgs eventArgs = new([1]);
        Assert.That(eventArgs.Data, Has.Length.EqualTo(1));
        Assert.That(eventArgs.Data[0], Is.EqualTo(1));
        Assert.That(eventArgs.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ConnectionDataReceivedEventArgs eventArgs = new([1]);
        ConnectionDataReceivedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));        
    }
}
