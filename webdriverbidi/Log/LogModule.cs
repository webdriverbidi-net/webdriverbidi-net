namespace WebDriverBidi.Log;

public sealed class LogModule : ProtocolModule
{
    public LogModule(Driver driver) : base(driver)
    {
        this.RegisterEventInvoker("log.entryAdded", typeof(LogEntry), this.OnEntryAdded);
    }

    public event EventHandler<EntryAddedEventArgs>? EntryAdded;

    private void OnEntryAdded(object eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a LogEntry object, so rather than duplicate the
        // properties to directly deserialize the EntryAddedEventArgs instance,
        // the protocol transport will deserialize to a LogEntry, then use that
        // here to create the appropriate EventArgs instance.
        var entry = eventData as LogEntry;
        if (entry is null)
        {
            throw new WebDriverBidiException("Unable to cast event data to LogEntry");
        }

        if (this.EntryAdded is not null)
        {
            this.EntryAdded(this, new EntryAddedEventArgs(entry));
        }
    }
}