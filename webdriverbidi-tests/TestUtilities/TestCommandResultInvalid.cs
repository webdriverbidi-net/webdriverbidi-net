namespace WebDriverBidi.TestUtilities;

using Newtonsoft.Json;

public class TestCommandResultInvalid: CommandResult
{
    [JsonProperty("value")]
    public string? Value { get; set; }
}