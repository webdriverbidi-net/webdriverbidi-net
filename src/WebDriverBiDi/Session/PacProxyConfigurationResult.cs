// <copyright file="PacProxyConfigurationResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// Object representing a read only proxy autoconfig used by the browser for this session.
/// </summary>
public class PacProxyConfigurationResult : ProxyConfigurationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PacProxyConfigurationResult"/> class.
    /// </summary>
    /// <param name="proxy">The proxy autoconfig proxy configuration.</param>
    public PacProxyConfigurationResult(PacProxyConfiguration proxy)
        : base(proxy)
    {
    }

    /// <summary>
    /// Gets the URL to the proxy autoconfig (PAC) settings.
    /// </summary>
    public string ProxyAutoConfigUrl => this.ProxyConfigurationAs<PacProxyConfiguration>().ProxyAutoConfigUrl;
}
