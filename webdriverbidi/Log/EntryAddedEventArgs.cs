namespace WebDriverBidi.Log;

using Newtonsoft.Json;
using Script;

[JsonObject(MemberSerialization.OptIn)]
public class EntryAddedEventArgs : EventArgs
{
    private LogEntry entry;

    internal EntryAddedEventArgs(LogEntry entry)
    {
        this.entry = entry;
    }

    public string Type => this.entry.Type;

    public Source Source => this.entry.Source;

    public string? Text => this.entry.Text;

    public long Timestamp => this.entry.Timestamp;

    public StackTrace? StackTrace => this.entry.StackTrace;

    public string? Method
    {
        get
        {
            var consoleLogEntry = this.entry as ConsoleLogEntry;
            if (consoleLogEntry is null)
            {
                return null;
            }

            return consoleLogEntry.Method;
        }
    }

    public IList<RemoteValue>? Arguments
    {
        get
        {
            var consoleLogEntry = this.entry as ConsoleLogEntry;
            if (consoleLogEntry is null)
            {
                return null;
            }

            return consoleLogEntry.Args;
        }
    }

}