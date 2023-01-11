namespace WebDriverBidi.TestUtilities;

using Newtonsoft.Json;

public class TestEventArgs: EventArgs
{
    private readonly string parameterName = "paramValue";

    [JsonProperty("paramName")]
    public string ParamName => parameterName;
}