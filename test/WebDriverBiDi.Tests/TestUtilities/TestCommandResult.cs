namespace WebDriverBiDi.TestUtilities;

using Newtonsoft.Json;

public class TestCommandResult: CommandResult
{
    private bool isError = false;

    public override bool IsError { get => this.isError; }

    [JsonProperty("value")]
    public string? Value { get; set; }

    [JsonProperty("elapsed")]
    public double? ElapsedMilliseconds { get; set; }

    public void SetIsErrorValue(bool isError)
    {
        this.isError = isError;
    }
}