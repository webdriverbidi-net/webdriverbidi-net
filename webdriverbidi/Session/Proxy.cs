namespace WebDriverBidi.Session;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class Proxy
{
    private ProxyType? type;
    private string? proxyAutoconfigUrl;
    private string? httpProxy;
    private string? sslProxy;
    private string? ftpProxy;
    private List<string>? noProxyAddresses;
    private string? socksProxy;
    private int? socksVersion;

    public static Proxy EmptyProxy => new();

    public ProxyType? Type { get => this.type; set => this.type = value; }

    [JsonProperty("proxyAutoconfigUrl", NullValueHandling = NullValueHandling.Ignore)]
    public string? ProxyAutoConfigUrl { get => this.proxyAutoconfigUrl; set => this.proxyAutoconfigUrl = value; }

    [JsonProperty("httpProxy", NullValueHandling = NullValueHandling.Ignore)]
    public string? HttpProxy { get => this.httpProxy; set => this.httpProxy = value; }

    [JsonProperty("sslProxy", NullValueHandling = NullValueHandling.Ignore)]
    public string? SslProxy { get => this.sslProxy; set => this.sslProxy = value; }

    [JsonProperty("ftpProxy", NullValueHandling = NullValueHandling.Ignore)]
    public string? FtpProxy { get => this.ftpProxy; set => this.ftpProxy = value; }

    [JsonProperty("socksProxy", NullValueHandling = NullValueHandling.Ignore)]
    public string? SocksProxy { get => this.socksProxy; set => this.socksProxy = value; }

    [JsonProperty("socksVersion", NullValueHandling = NullValueHandling.Ignore)]
    public int? SocksVersion { get => this.socksVersion; set => this.socksVersion = value; }

    [JsonProperty("noProxy", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? NoProxyAddresses { get => this.noProxyAddresses; set => this.noProxyAddresses = value; }

    [JsonProperty("proxyType", NullValueHandling = NullValueHandling.Ignore)]
    internal string? SerializableProxyType
    {
        get
        {
            if (this.type is null)
            {
                return null;
            }

            if (this.type.Value == ProxyType.ProxyAutoConfig)
            {
                return "pac";
            }

            return this.type.Value.ToString().ToLowerInvariant();
        }

        set
        {
            if (value is null)
            {
                this.type = null;
                return;
            }

            if (value == "pac")
            {
                this.type = ProxyType.ProxyAutoConfig;
                return;
            }

            if (value.ToLowerInvariant() == "proxyautoconfig")
            {
                // The value 'proxyautoconfig' is expressly invalid in the spec
                throw new WebDriverBidiException($"Invalid value for proxy type: '{value}'");
            }

            if (!Enum.TryParse<ProxyType>(value, true, out ProxyType deserializedType))
            {
                throw new WebDriverBidiException($"Invalid value for proxy type: '{value}'");
            }

            this.type = deserializedType;
        }
    }
}