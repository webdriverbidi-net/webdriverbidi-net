namespace WebDriverBidi.Session;

using System;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class UnsubscribeCommandSettings : CommandSettings
{
    private List<string> eventList = new List<string>();

    private List<string> contextList = new List<string>();

    public UnsubscribeCommandSettings()
    {
    }

    public override string MethodName => "session.unsubscribe";

    public override Type ResultType => typeof(EmptyResult);

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