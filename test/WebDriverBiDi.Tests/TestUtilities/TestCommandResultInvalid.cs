namespace WebDriverBiDi.TestUtilities;

using System.Text.Json.Serialization;

public record TestCommandResultInvalid: CommandResult
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
