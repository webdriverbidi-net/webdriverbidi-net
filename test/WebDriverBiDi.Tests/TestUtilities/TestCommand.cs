namespace WebDriverBiDi.TestUtilities;

using Newtonsoft.Json;
using WebDriverBiDi.Protocol;

public class TestCommand : CommandParameters<TestCommandResult>
{
    private readonly string parameterName = "parameterValue";
    private readonly string commandName;

    public TestCommand(string commandName)
    {
        this.commandName = commandName;
    }

    public override string MethodName => this.commandName;

    public override Type ResponseType => typeof(CommandResponseMessage<TestCommandResult>);

    [JsonProperty("parameterName")]
    public string ParameterName => this.parameterName;
}