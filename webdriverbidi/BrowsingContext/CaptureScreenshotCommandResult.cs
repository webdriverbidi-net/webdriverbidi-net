namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CaptureScreenshotCommandResult : CommandResult
{
    private string base64Screenshot;

    [JsonConstructor]
    private CaptureScreenshotCommandResult(string base64Screenshot)
    {
        this.base64Screenshot = base64Screenshot;
    }

    [JsonProperty("data")]
    [JsonRequired]
    public string Data { get => this.base64Screenshot; internal set => this.base64Screenshot = value; }
}
