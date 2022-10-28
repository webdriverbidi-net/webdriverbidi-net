namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using JsonConverters;

[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(CapabilitiesResultJsonConverter))]
public class CapabilitiesResult
{
    private bool acceptInsecureCertificates = false;
    private string browserName = string.Empty;
    private string browserVersion = string.Empty;
    private string platformName = string.Empty;
    private bool setWindowRect = false;
    private ProxyResult? proxyResult;
    private Proxy proxy = WebDriverBidi.Session.Proxy.EmptyProxy;
    private AdditionalCapabilities additionalCapabilities = AdditionalCapabilities.EmptyAdditionalCapabilities;

    [JsonProperty("acceptInsecureCerts")]
    [JsonRequired]
    public bool AcceptInsecureCertificates { get => this.acceptInsecureCertificates; internal set => this.acceptInsecureCertificates = value; }

    [JsonProperty("browserName")]
    [JsonRequired]
    public string BrowserName { get => this.browserName; internal set => this.browserName = value; }

    [JsonProperty("browserVersion")]
    [JsonRequired]
    public string BrowserVersion { get => this.browserVersion; internal set => this.browserVersion = value; }

    [JsonProperty("platformName")]
    [JsonRequired]
    public string PlatformName { get => this.platformName; internal set => this.platformName = value; }

    [JsonProperty("setWindowRect")]
    [JsonRequired]
    public bool SetWindowRect { get => this.setWindowRect; internal set => this.setWindowRect = value; }

    public AdditionalCapabilities AdditionalCapabilities { get => this.additionalCapabilities; internal set => this.additionalCapabilities = value; }

    public ProxyResult Proxy
    {
        get 
        {
            if (this.proxyResult is null)
            {
                this.proxyResult = new ProxyResult(this.proxy);
            }

            return this.proxyResult;
        }
    }

    [JsonProperty("proxy")]
    [JsonRequired]
    internal Proxy SerializableProxy { get => this.proxy; set => this.proxy = value; }
}