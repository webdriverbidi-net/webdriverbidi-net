// <copyright file="ProxyConfiguration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object representing a proxy to be used by the browser.
/// </summary>
[JsonConverter(typeof(ProxyConfigurationJsonConverter))]
public class ProxyConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyConfiguration"/> class.
    /// </summary>
    /// <param name="proxyType">The type of proxy configuration.</param>
    protected ProxyConfiguration(ProxyType proxyType)
    {
        this.ProxyType = proxyType;
    }

    /// <summary>
    /// Gets the type of proxy configuration.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("proxyType")]
    [JsonInclude]
    public ProxyType ProxyType { get; internal set; }

    /// <summary>
    /// Gets read-only dictionary of additional properties deserialized with this message.
    /// </summary>
    [JsonExtensionData]
    [JsonInclude]
    public Dictionary<string, object?> AdditionalData { get; } = [];
}
