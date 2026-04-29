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
        return this.GetEventsForEventName(TimeSpan.Zero, eventNames);
    }

    public List<EventWrittenEventArgs> GetEventsForEventName(TimeSpan timeout, params string[] eventNames)
    {
        DateTime timeoutTime = DateTime.Now.Add(timeout);
        List<EventWrittenEventArgs> foundEvents;
        lock (this.events)
        {
            foundEvents = this.events.Where(e => eventNames.Contains(e.EventName)).ToList();
        }

        while (timeout > TimeSpan.Zero && foundEvents.Count == 0 && DateTime.Now <= timeoutTime)
        {
            lock (this.events)
            {
                foundEvents = this.events.Where(e => eventNames.Contains(e.EventName)).ToList();
            }
        }

        return foundEvents;
    }

    public void ClearEvents()
    {
        lock (this.events)
        {
            this.events.Clear();
        }
    }
}
