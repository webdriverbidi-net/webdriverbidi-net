namespace WebDriverBiDi.TestUtilities;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class TestEventArgs: WebDriverBiDiEventArgs
{
    private readonly string parameterName = "paramValue";

    [JsonProperty("paramName")]
    [JsonRequired]
    public string ParamName => parameterName;
}
