namespace WebDriverBiDi.TestUtilities;

using System.Text.Json.Serialization;

public class TestEventArgs: WebDriverBiDiEventArgs
{
    private string parameterName = "paramValue";

    [JsonPropertyName("paramName")]
    [JsonRequired]
    public string ParamName { get => parameterName; set => parameterName = value; }
}
