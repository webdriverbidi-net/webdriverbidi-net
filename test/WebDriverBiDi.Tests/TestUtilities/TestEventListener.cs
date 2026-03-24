namespace WebDriverBiDi.TestUtilities;

using System.Diagnostics.Tracing;

/// <summary>
/// Test EventListener that captures WebDriverBiDi events.
/// </summary>
public class TestEventListener : EventListener
{
    private readonly List<EventWrittenEventArgs> events = new();

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "WebDriverBiDi")
        {
            this.EnableEvents(eventSource, EventLevel.Verbose);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventSource.Name == "WebDriverBiDi")
        {
            lock (this.events)
            {
                this.events.Add(eventData);
            }
        }
    }

    public List<EventWrittenEventArgs> GetEventsForEventName(params string[] eventNames)
    {
        lock (this.events)
        {
            return this.events.Where(e => eventNames.Contains(e.EventName)).ToList();
        }
    }

    public void ClearEvents()
    {
        lock (this.events)
        {
            this.events.Clear();
        }
    }
}
