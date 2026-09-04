namespace WebDriverBiDi.TestUtilities;

using System.Diagnostics.Tracing;

/// <summary>
/// Test EventListener that captures WebDriverBiDi events.
/// </summary>
public class TestEventListener : EventListener
{
    private readonly object eventListObject = new();
    private readonly List<EventWrittenEventArgs> events = new();
    private readonly EventLevel minimumLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestEventListener"/> class.
    /// </summary>
    /// <param name="minimumLevel">The level to subscribe at. Defaults to capturing everything.</param>
    public TestEventListener(EventLevel minimumLevel = EventLevel.Verbose)
    {
        this.minimumLevel = minimumLevel;
        this.EnableEvents(WebDriverBiDiEventSource.RaiseEvent, minimumLevel);
    }

    /// <summary>
    /// Gets a snapshot of the events captured so far, in the order they were written.
    /// </summary>
    /// <remarks>
    /// A copy rather than the live list: events arrive on the thread that raised them, so handing out
    /// the backing list would let a caller enumerate it while it is being appended to.
    /// </remarks>
    public IReadOnlyList<EventWrittenEventArgs> Events
    {
        get
        {
            lock (this.eventListObject)
            {
                return this.events.ToList();
            }
        }
    }

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "WebDriverBiDi")
        {
            // OnEventSourceCreated runs from the base constructor, before this instance's field is
            // assigned, so a source that already exists is enabled at the default EventLevel.LogAlways.
            // Re-subscribing at the configured level corrects that; enabling an already-enabled source
            // updates its level rather than adding a second subscription.
            this.EnableEvents(eventSource, this.minimumLevel);
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

    /// <summary>
    /// Blocks until at least <paramref name="count"/> events with the given name have been written, or
    /// the safety bound elapses, and returns whatever has been collected.
    /// </summary>
    /// <param name="eventName">The event name to wait for.</param>
    /// <param name="count">The number of events to wait for.</param>
    /// <param name="timeout">A safety bound. The wait ends when the events arrive, not when this elapses.</param>
    /// <returns>The matching events collected so far.</returns>
    /// <remarks>
    /// The single-argument overload above waits only for the first match, which is why tests that needed a
    /// second event resorted to polling loops. Waiting on the monitor that <see cref="OnEventWritten"/>
    /// pulses means the wait ends when the event is written rather than on the next poll tick, and the
    /// elapsed time is measured with a <see cref="System.Diagnostics.Stopwatch"/> so a system clock
    /// adjustment cannot shorten or extend it.
    /// </remarks>
    public List<EventWrittenEventArgs> WaitForEventCount(string eventName, int count, TimeSpan timeout)
    {
        System.Diagnostics.Stopwatch elapsed = System.Diagnostics.Stopwatch.StartNew();
        lock (this.eventListObject)
        {
            List<EventWrittenEventArgs> foundEvents = this.events.Where(e => e.EventName == eventName).ToList();
            while (foundEvents.Count < count)
            {
                TimeSpan remaining = timeout - elapsed.Elapsed;
                if (remaining <= TimeSpan.Zero)
                {
                    break;
                }

                Monitor.Wait(this.eventListObject, remaining);
                foundEvents = this.events.Where(e => e.EventName == eventName).ToList();
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
