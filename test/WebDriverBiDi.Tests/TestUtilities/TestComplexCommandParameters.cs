namespace WebDriverBiDi.TestUtilities;

using System.Text.Json.Serialization;

public class TestComplexCommandParameters : CommandParameters<TestCommandResult>
{
    private string parameterName = "parameterValue";
    private List<object?> complexValue = [
        "stringValue",
        1,
        2.3d,
        true,
        null
    ];
    private readonly string commandName;

    public TestComplexCommandParameters(string commandName, string parameterValue = "parameterValue")
    {
        this.commandName = commandName;
        this.parameterName = parameterValue;
    }

    [JsonIgnore]
    public override string MethodName => this.commandName;

    [JsonPropertyName("parameterName")]
    public string ParameterName { get => this.parameterName; }

    [JsonPropertyName("complex")]
    public List<object?> ComplexValue => this.complexValue;
}
