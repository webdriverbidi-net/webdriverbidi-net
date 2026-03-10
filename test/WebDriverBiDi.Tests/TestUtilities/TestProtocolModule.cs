namespace WebDriverBiDi.TestUtilities;

public sealed class TestProtocolModule : Module
{
    private const string EventName = "protocol.event";

    /// <summary>
    /// Initializes a new instance of the <see cref="TestProtocolModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    public TestProtocolModule(IBiDiCommandExecutor driver, uint maxObserverCount = 0, bool registerEvents = true)
        : base(driver)
    {
        this.OnEventInvoked = new ObservableEvent<TestEventArgs>(EventName, maxObserverCount);
        if (registerEvents)
        {
            this.RegisterObservableEvent(this.OnEventInvoked);
        }
    }

    public ObservableEvent<TestEventArgs> OnEventInvoked { get; }

    public override string ModuleName => "protocol";

    public IBiDiCommandExecutor HostingDriver => this.Driver;
}
