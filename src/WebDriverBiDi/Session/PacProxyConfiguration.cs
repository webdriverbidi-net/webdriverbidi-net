// <copyright file="PacProxyConfiguration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Object representing a proxy autoconfig proxy to be used by the browser.
/// </summary>
public class PacProxyConfiguration : ProxyConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PacProxyConfiguration"/> class.
    /// </summary>
    /// <param name="proxyAutoConfigUrl">The URL to the proxy autoconfig file.</param>
    public PacProxyConfiguration(string proxyAutoConfigUrl)
        : base(ProxyType.ProxyAutoConfig)
    {
        this.ProxyAutoConfigUrl = proxyAutoConfigUrl;
    }

    /// <summary>
    /// Gets or sets the URL to the proxy autoconfig (PAC) settings.
    /// </summary>
    [JsonPropertyName("proxyAutoconfigUrl")]
    [JsonRequired]
    public string ProxyAutoConfigUrl { get; set; }
}
