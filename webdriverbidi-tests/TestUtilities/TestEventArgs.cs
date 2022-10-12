namespace WebDriverBidi.TestUtilities;

using Newtonsoft.Json;

public class TestEventArgs: EventArgs
{
    [JsonProperty("paramName")]
    public string ParamName => "paramValue";
}