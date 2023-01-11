namespace WebDriverBidi.Script;

using System;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class DisownCommandSettings : CommandSettings
{
    private List<string> handles = new();
    private ScriptTarget target;

    public DisownCommandSettings(ScriptTarget target, params string[] handleValues)
    {
        this.target = target;
        this.handles.AddRange(handleValues);
    }

    public override string MethodName => "script.disown";

    public override Type ResultType => typeof(EmptyResult);

    [JsonProperty("target")]
    public ScriptTarget Target { get => this.target; set => this.target = value; }

    [JsonProperty("handles")]
    public List<string> Handles { get => handles; set => this.handles = value; }
}