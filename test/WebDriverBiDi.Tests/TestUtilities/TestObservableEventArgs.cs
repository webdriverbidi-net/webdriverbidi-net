namespace WebDriverBiDi.TestUtilities;

public record TestObservableEventArgs : WebDriverBiDiEventArgs
{
    private readonly string eventValue = string.Empty;

    public TestObservableEventArgs(string eventValue)
    {
        this.eventValue = eventValue;
    }

    public string EventValue => this.eventValue;
}
