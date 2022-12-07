namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CaptureScreenshotCommandResult : CommandResult
{
    private string base64Screenshot = string.Empty;

    [JsonConstructor]
    private CaptureScreenshotCommandResult()
    {
    }

    [JsonProperty("data")]
    [JsonRequired]
    public string Data { get => this.base64Screenshot; internal set => this.base64Screenshot = value; }
}
