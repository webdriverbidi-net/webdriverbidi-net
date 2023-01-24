namespace WebDriverBidi.TestUtilities;

public sealed class TestProtocolModule : Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestProtocolModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public TestProtocolModule(Driver driver)
        : base(driver)
    {
        this.RegisterEventInvoker<TestEventArgs>("protocol.event", this.OnEventInvoked);
    }

    public event EventHandler<TestEventArgs>? EventInvoked;

    public override string ModuleName => "protocol";

    private void OnEventInvoked(EventInfo<TestEventArgs> eventData)
    {
        if (this.EventInvoked is not null)
        {
            TestEventArgs eventArgs = eventData.EventData;
            eventArgs.AdditionalData = eventData.AdditionalData;
            this.EventInvoked(this, eventArgs);
        }
    }
}

