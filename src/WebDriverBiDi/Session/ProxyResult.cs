// <copyright file="ProxyResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// A read-only object of proxy settings returned from the remote end.
/// </summary>
public class ProxyResult
{
    private readonly Proxy proxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyResult"/> class.
    /// </summary>
    /// <param name="proxy">The deserialized proxy object sent by the remote end.</param>
    internal ProxyResult(Proxy proxy)
    {
        this.proxy = proxy;
    }

    /// <summary>
    /// Gets the type of proxy.
    /// </summary>
    public ProxyType? Type => this.proxy.Type;

    /// <summary>
    /// Gets the URL to the proxy autoconfig (PAC) settings.
    /// </summary>
    public string? ProxyAutoConfigUrl => this.proxy.ProxyAutoConfigUrl;

    /// <summary>
    /// Gets the address to be used to proxy HTTP commands.
    /// </summary>
    public string? HttpProxy => this.proxy.HttpProxy;

    /// <summary>
    /// Gets the address to be used to proxy HTTPS commands.
    /// </summary>
    public string? SslProxy => this.proxy.SslProxy;

    /// <summary>
    /// Gets the address to be used to proxy FTP commands.
    /// </summary>
    public string? FtpProxy => this.proxy.FtpProxy;

    /// <summary>
    /// Gets the address of a SOCKS proxy used to proxy commands.
    /// </summary>
    public string? SocksProxy => this.proxy.SocksProxy;

    /// <summary>
    /// Gets the version of the SOCKS proxy to be used.
    /// </summary>
    public int? SocksVersion => this.proxy.SocksVersion;

    /// <summary>
    /// Gets a read-only list of addresses to be bypassed by the proxy.
    /// </summary>
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