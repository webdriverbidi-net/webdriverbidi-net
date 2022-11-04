namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class StackTrace
{
    private List<StackFrame> callFrames = new List<StackFrame>();

    [JsonConstructor]
    internal StackTrace()
    {
    }

    public IList<StackFrame> CallFrames => this.callFrames.AsReadOnly();

    [JsonProperty("callFrames")]
    [JsonRequired]
    internal List<StackFrame> SerializableCallFrames { get => this.callFrames; set => this.callFrames = value; }
}