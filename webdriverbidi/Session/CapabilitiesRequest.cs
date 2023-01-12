// <copyright file="CapabilitiesRequest.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Object containing capabilities requested when starting a new session.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(CapabilitiesRequestJsonConverter))]
public class CapabilitiesRequest
{
    private readonly Dictionary<string, object?> additionalCapabilities = new();
    private bool? acceptInsecureCertificates;
    private string? browserName;
    private string? browserVersion;
    private string? platformName;
    private Proxy? proxy;

    /// <summary>
    /// Gets or sets a value indicating whether the browser should accept insecure (self-signed) SSL certificates.
    /// </summary>
    [JsonProperty("acceptInsecureCerts", NullValueHandling = NullValueHandling.Ignore)]
    public bool? AcceptInsecureCertificates { get => this.acceptInsecureCertificates; set => this.acceptInsecureCertificates = value; }

    /// <summary>
    /// Gets or sets the name of the browser.
    /// </summary>
    [JsonProperty("browserName", NullValueHandling = NullValueHandling.Ignore)]
    public string? BrowserName { get => this.browserName; set => this.browserName = value; }

    /// <summary>
    /// Gets or sets the version of the browser.
    /// </summary>
    [JsonProperty("browserVersion", NullValueHandling = NullValueHandling.Ignore)]
    public string? BrowserVersion { get => this.browserVersion; set => this.browserVersion = value; }

    /// <summary>
    /// Gets or sets the platform name.
    /// </summary>
    [JsonProperty("platformName", NullValueHandling = NullValueHandling.Ignore)]
    public string? PlatformName { get => this.platformName; set => this.platformName = value; }

    /// <summary>
    /// Gets or sets the proxy to use with this session.
    /// </summary>
    [JsonProperty("proxy", NullValueHandling = NullValueHandling.Ignore)]
    public Proxy? Proxy { get => this.proxy; set => this.proxy = value; }

    /// <summary>
    /// Gets the dictionary containing additional capabilities to use with this session.
    /// </summary>
    public Dictionary<string, object?> AdditionalCapabilities => this.additionalCapabilities;
}
