namespace WebDriverBiDi.TestUtilities;

using System.Text.Json;
using System.Text.Json.Serialization;

public class TestCommandResult: CommandResult
{
    private bool isError = false;

    [JsonIgnore]
    public override bool IsError { get => this.isError; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    public void SetIsErrorValue(bool isError)
    {
        this.isError = isError;
    }
}