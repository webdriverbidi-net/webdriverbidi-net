namespace WebDriverBidi.TestUtilities;

public class TestParameterizedEventArgs : WebDriverBidiEventArgs
{
    private readonly string eventName;

    public TestParameterizedEventArgs(TestValidEventData data)
    {
        this.eventName = data.Name;
    }

    public string EventName => this.eventName;
}
