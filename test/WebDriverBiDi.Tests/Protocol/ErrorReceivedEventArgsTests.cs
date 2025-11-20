namespace WebDriverBiDi.Protocol;

[TestFixture]
public class ErrorReceivedEventArgsTests
{
    [Test]
    public void TestCanCreateErrorReceivedEventArgsWithNullErrorData()
    {
        ErrorReceivedEventArgs eventArgs = new(null);
        Assert.That(eventArgs.ErrorData, Is.Null);
        Assert.That(eventArgs.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ErrorReceivedEventArgs eventArgs = new(null);
        ErrorReceivedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));        
    }
}
