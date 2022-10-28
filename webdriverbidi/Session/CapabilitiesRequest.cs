namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using JsonConverters;

[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(CapabilitiesRequestJsonConverter))]
public class CapabilitiesRequest
{
    private bool? acceptInsecureCertificates;
    private string? browserName;
    private string? browserVersion;
    private string? platformName;
    private Proxy? proxy;
    private Dictionary<string, object?> additionalCapabilities = new Dictionary<string, object?>();

    [JsonProperty("acceptInsecureCerts", NullValueHandling = NullValueHandling.Ignore)]
    public bool? AcceptInsecureCertificates { get => this.acceptInsecureCertificates; set => this.acceptInsecureCertificates = value; }

    [JsonProperty("browserName", NullValueHandling = NullValueHandling.Ignore)]
    public string? BrowserName { get => this.browserName; set => this.browserName = value; }

    [JsonProperty("browserVersion", NullValueHandling = NullValueHandling.Ignore)]
    public string? BrowserVersion { get => this.browserVersion; set => this.browserVersion = value; }

    [JsonProperty("platformName", NullValueHandling = NullValueHandling.Ignore)]
    public string? PlatformName { get => this.platformName; set => this.platformName = value; }

    [JsonProperty("proxy", NullValueHandling = NullValueHandling.Ignore)]
    public Proxy? Proxy { get => this.proxy; set => this.proxy = value; }

    public Dictionary<string, object?> AdditionalCapabilities => this.additionalCapabilities;
}