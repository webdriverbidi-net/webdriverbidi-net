namespace WebDriverBiDi.TestUtilities;

public sealed class TestProtocolModule : Module
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestProtocolModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public TestProtocolModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterAsyncEventInvoker<TestEventArgs>("protocol.event", this.OnEventInvoked);
    }

    public event EventHandler<TestEventArgs>? EventInvoked;

    public override string ModuleName => "protocol";

    private Task OnEventInvoked(EventInfo<TestEventArgs> eventData)
    {
        if (this.EventInvoked is not null)
        {
            TestEventArgs eventArgs = eventData.EventData;
            eventArgs.AdditionalData = eventData.AdditionalData;
            this.EventInvoked(this, eventArgs);
        }

        return Task.CompletedTask;
    }
}
