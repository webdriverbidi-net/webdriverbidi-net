namespace WebDriverBiDi.TestUtilities;

using Newtonsoft.Json;
using WebDriverBiDi.Protocol;

public class TestCommandParameters : CommandParameters<TestCommandResult>
{
    private readonly string parameterName;
    private readonly string commandName;

    public TestCommandParameters(string commandName, string parameterValue = "parameterValue")
    {
        this.commandName = commandName;
        this.parameterName = parameterValue;
    }

    public override string MethodName => this.commandName;

    public override Type ResponseType => typeof(CommandResponseMessage<TestCommandResult>);

    [JsonProperty("parameterName")]
    public string ParameterName => this.parameterName;
}
