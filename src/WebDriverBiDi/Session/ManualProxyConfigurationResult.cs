// <copyright file="ManualProxyConfigurationResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// Object representing a read only proxy autoconfig used by the browser for this session.
/// </summary>
public record ManualProxyConfigurationResult : ProxyConfigurationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManualProxyConfigurationResult"/> class.
    /// </summary>
    /// <param name="proxy">The proxy autoconfig proxy configuration.</param>
    public ManualProxyConfigurationResult(ManualProxyConfiguration proxy)
        : base(proxy)
    {
    }

    /// <summary>
    /// Gets the address to be used to proxy HTTP commands.
    /// </summary>
    public string? HttpProxy => this.ProxyConfiguration.HttpProxy;

    /// <summary>
    /// Gets the address to be used to proxy HTTPS commands.
    /// </summary>
    public string? SslProxy => this.ProxyConfiguration.SslProxy;

    /// <summary>
    /// Gets the address of a SOCKS proxy used to proxy commands.
    /// </summary>
    public string? SocksProxy => this.ProxyConfiguration.SocksProxy;

    /// <summary>
    /// Gets the version of the SOCKS proxy to be used.
    /// </summary>
    public int? SocksVersion => this.ProxyConfiguration.SocksVersion;

    /// <summary>
    /// Gets a list of addresses to be bypassed by the proxy.
    /// </summary>
    public List<string>? NoProxyAddresses => this.ProxyConfiguration.NoProxyAddresses;

    private ManualProxyConfiguration ProxyConfiguration => this.ProxyConfigurationAs<ManualProxyConfiguration>();
}
