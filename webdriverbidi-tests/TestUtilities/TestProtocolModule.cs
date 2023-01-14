namespace WebDriverBidi.TestUtilities;

public sealed class TestProtocolModule : ProtocolModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public TestProtocolModule(Driver driver)
        : base(driver)
    {
        this.RegisterEventInvoker("protocol.event", typeof(TestEventArgs), this.OnEventInvoked);
    }

    public event EventHandler<TestEventArgs>? EventInvoked;

    private void OnEventInvoked(object eventData)
    {
        if (eventData is TestEventArgs entry)
        {
            if (this.EventInvoked is not null)
            {
                this.EventInvoked(this, new TestEventArgs());
            }
        }

    }
}

