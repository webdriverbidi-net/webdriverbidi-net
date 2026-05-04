namespace WebDriverBiDi.TestUtilities;

public sealed class TestProtocolModule : Module
{
    private const string EventName = "protocol.event";

    private readonly ObservableEventInvocable<TestEventArgs> invocableTestObservableEvent;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestProtocolModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    public TestProtocolModule(IBiDiCommandExecutor driver, uint maxObserverCount = 0, bool registerEvents = true)
        : base(driver)
    {
        this.invocableTestObservableEvent = new ObservableEventInvocable<TestEventArgs>(EventName, maxObserverCount);
        if (registerEvents)
        {
            this.RegisterObservableEvent(this.invocableTestObservableEvent);
        }
    }

    public ObservableEvent<TestEventArgs> OnEventInvoked => this.invocableTestObservableEvent;

    public override string ModuleName => "protocol";

    public IBiDiCommandExecutor HostingDriver => this.Driver;
}
