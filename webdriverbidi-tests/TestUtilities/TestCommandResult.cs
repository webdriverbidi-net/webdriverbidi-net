namespace WebDriverBidi.TestUtilities;

using Newtonsoft.Json;

public class TestCommandResult: ResponseData
{
    [JsonProperty("value")]
    public string? Value { get; set; }
}