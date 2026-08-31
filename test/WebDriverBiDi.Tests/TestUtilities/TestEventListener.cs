namespace WebDriverBiDi.TestUtilities;

using System.Diagnostics.Tracing;

/// <summary>
/// Test EventListener that captures WebDriverBiDi events.
/// </summary>
public class TestEventListener : EventListener
{
    private readonly object eventListObject = new();
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
            lock (this.eventListObject)
            {
                this.events.Add(eventData);

                // Wake any waiter in GetEventsForEventName so it can re-check instead of busy-spinning.
                Monitor.PulseAll(this.eventListObject);
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
        lock (this.eventListObject)
        {
            List<EventWrittenEventArgs> foundEvents = this.events.Where(e => eventNames.Contains(e.EventName)).ToList();

            // Block on the monitor (released while waiting, signalled by OnEventWritten) rather than
            // spinning; wake on a new event or when the remaining timeout elapses.
            while (timeout > TimeSpan.Zero && foundEvents.Count == 0)
            {
                TimeSpan remaining = timeoutTime - DateTime.Now;
                if (remaining <= TimeSpan.Zero)
                {
                    break;
                }

                Monitor.Wait(this.eventListObject, remaining);
                foundEvents = this.events.Where(e => eventNames.Contains(e.EventName)).ToList();
            }

            return foundEvents;
        }
    }

    public void ClearEvents()
    {
        lock (this.eventListObject)
        {
            this.events.Clear();
        }
    }
}
