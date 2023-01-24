namespace WebDriverBidi.TestUtilities;

using Newtonsoft.Json;

public class TestCommandResult: CommandResult
{
    [JsonProperty("value")]
    public string? Value { get; set; }
}