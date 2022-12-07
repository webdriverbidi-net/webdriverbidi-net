namespace WebDriverBidi.Session;

public class ProxyResult
{
    private Proxy proxy;

    internal ProxyResult(Proxy proxy)
    {
        this.proxy = proxy;
    }

    public ProxyType? Type => this.proxy.Type;

    public string? ProxyAutoConfigUrl => this.proxy.ProxyAutoConfigUrl;

    public string? HttpProxy => this.proxy.HttpProxy;

    public string? SslProxy => this.proxy.SslProxy;

    public string? FtpProxy => this.proxy.FtpProxy;

    public string? SocksProxy => this.proxy.SocksProxy;

    public int? SocksVersion => this.proxy.SocksVersion;

    public IList<string>? NoProxyAddresses
    {
        get
        {
            if (this.proxy.NoProxyAddresses is null)
            {
                return null;
            }

            return this.proxy.NoProxyAddresses.AsReadOnly();
        }
    }
}