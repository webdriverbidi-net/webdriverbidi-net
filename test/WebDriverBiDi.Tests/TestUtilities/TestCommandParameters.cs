namespace WebDriverBiDi.TestUtilities;

using System.Text.Json.Serialization;
using WebDriverBiDi.Protocol;

public class TestCommandParameters : CommandParameters<TestCommandResult>
{
    private string parameterName = "parameterValue";
    private readonly string commandName;

    public TestCommandParameters(string commandName, string parameterValue = "parameterValue")
    {
        this.commandName = commandName;
        this.parameterName = parameterValue;
    }

    [JsonIgnore]
    public override string MethodName => this.commandName;

    [JsonPropertyName("parameterName")]
    public string ParameterName { get => this.parameterName; set => this.parameterName = value; }
}
