namespace WebDriverBidi.TestUtilities;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class TestEventArgs: EventArgs
{
    private readonly string parameterName = "paramValue";

    [JsonProperty("paramName")]
    [JsonRequired]
    public string ParamName => parameterName;
}