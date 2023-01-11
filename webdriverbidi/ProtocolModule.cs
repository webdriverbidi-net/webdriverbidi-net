namespace WebDriverBidi;

public class ProtocolModule
{
    private readonly Driver driver;
    private readonly Dictionary<string, WebDriverBidiEventData> eventInvokers = new();

    protected ProtocolModule(Driver driver)
    {
        this.driver = driver;
        this.driver.EventReceived += OnDriverEventReceived;
    }

    protected Driver Driver => this.driver;

    protected void RegisterEventInvoker(string eventName, Type eventArgsType, Action<object> eventInvoker)
    {
        this.eventInvokers[eventName] = new WebDriverBidiEventData(eventArgsType, eventInvoker);
        this.driver.RegisterEvent(eventName, eventArgsType);
    }

    private void OnDriverEventReceived(object? sender, ProtocolEventReceivedEventArgs e)
    {
        if (this.eventInvokers.ContainsKey(e.EventName))
        {
            var eventData = eventInvokers[e.EventName];
            var eventArgs = e.EventData;
            if (eventArgs is null || !eventArgs.GetType().IsAssignableTo(eventData.EventArgsType))
            {
                throw new WebDriverBidiException($"Unable to cast received event data to {eventData.EventArgsType.Name}");
            }
            
            eventData.EventInvoker(eventArgs);
        }
    }
}