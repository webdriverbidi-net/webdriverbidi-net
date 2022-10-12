namespace WebDriverBidi.BrowsingContext;

using System;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CaptureScreenshotCommandSettings : CommandSettings
{
    private string browsingContextId;

    public CaptureScreenshotCommandSettings(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    public override string MethodName => "browsingContext.captureScreenshot";

    public override Type ResultType => typeof(CaptureScreenshotCommandResult);

    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }
}