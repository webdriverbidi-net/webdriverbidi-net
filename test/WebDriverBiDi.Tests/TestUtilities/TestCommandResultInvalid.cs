namespace WebDriverBiDi.TestUtilities;

using System.Text.Json.Serialization;

public class TestCommandResultInvalid: CommandResult
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
