namespace WebDriverBiDi;

[TestFixture]
public class ObservableEventNameAttributeTests
{
    [Test]
    public void TestCanGetEventName()
    {
        ObservableEventNameAttribute attribute = new("test event name");
        Assert.That(attribute.EventName, Is.EqualTo("test event name"));
    }
}
