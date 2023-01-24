namespace WebDriverBidi.TestUtilities;

using Newtonsoft.Json;

public class TestCommand : CommandData<TestCommandResult>
{
    private readonly string parameterName = "parameterValue";
    private readonly string commandName;

    public TestCommand(string commandName)
    {
        this.commandName = commandName;
    }

    public override string MethodName => this.commandName;

    public override Type ResponseType => typeof(CommandResponse<TestCommandResult>);

    [JsonProperty("parameterName")]
    public string ParameterName => this.parameterName;
}