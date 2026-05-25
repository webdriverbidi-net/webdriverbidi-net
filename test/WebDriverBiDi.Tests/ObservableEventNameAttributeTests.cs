namespace WebDriverBiDi;

public class ObservableEventNameAttributeTests
{
    [Fact]
    public void TestCanGetEventName()
    {
        ObservableEventNameAttribute attribute = new("test event name");
        Assert.Equal("test event name", attribute.EventName);
    }
}
