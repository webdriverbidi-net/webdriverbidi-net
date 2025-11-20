namespace WebDriverBiDi.TestUtilities;

public sealed class TestProtocolModule : Module
{
    private ObservableEvent<TestEventArgs> onEventInvokedEvent;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestProtocolModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public TestProtocolModule(BiDiDriver driver, int maxObserverCount = 0)
        : base(driver)
    {
        this.onEventInvokedEvent = new ObservableEvent<TestEventArgs>("protocol.event", maxObserverCount);
        this.RegisterAsyncEventInvoker<TestEventArgs>("protocol.event", this.OnEventInvokedAsync);
    }

    public ObservableEvent<TestEventArgs> OnEventInvoked => this.onEventInvokedEvent;

    public override string ModuleName => "protocol";

    public BiDiDriver HostingDriver => this.Driver;

    private async Task OnEventInvokedAsync(EventInfo<TestEventArgs> eventData)
    {
        TestEventArgs eventArgs = eventData.EventData;
        eventArgs.AdditionalData = eventData.AdditionalData;
        await this.onEventInvokedEvent.NotifyObserversAsync(eventArgs);
    }
}
