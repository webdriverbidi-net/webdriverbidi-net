namespace WebDriverBiDi.TestUtilities;

public record TestParameterizedEventArgs : WebDriverBiDiEventArgs
{
    private readonly string eventName;

    public TestParameterizedEventArgs(TestValidEventData data)
    {
        this.eventName = data.Name;
    }

    public string EventName => this.eventName;
}
