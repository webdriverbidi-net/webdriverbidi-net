namespace WebDriverBidi.Session;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class UnsubscribeCommandProperties : CommandProperties
{
    private List<string> eventList = new List<string>();

    private List<string> contextList = new List<string>();

    public UnsubscribeCommandProperties()
    {
    }

    public override string MethodName => "session.unsubscribe";

    [JsonProperty("events")]
    public List<string> Events => this.eventList;

    public List<string> Contexts => this.contextList;

    [JsonProperty("contexts", NullValueHandling = NullValueHandling.Ignore)]
    internal List<string>? SeralizableContexts
    {
        get
        {
            if (this.contextList.Count == 0)
            {
                return null;
            }

            return this.contextList;
        }
    }
}