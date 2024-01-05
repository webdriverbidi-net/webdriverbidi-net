// <copyright file="CapabilitiesRequest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing capabilities requested when starting a new session.
/// </summary>
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
    [JsonPropertyName("acceptInsecureCerts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AcceptInsecureCertificates { get => this.acceptInsecureCertificates; set => this.acceptInsecureCertificates = value; }

    /// <summary>
    /// Gets or sets the name of the browser.
    /// </summary>
    [JsonPropertyName("browserName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BrowserName { get => this.browserName; set => this.browserName = value; }

    /// <summary>
    /// Gets or sets the version of the browser.
    /// </summary>
    [JsonPropertyName("browserVersion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BrowserVersion { get => this.browserVersion; set => this.browserVersion = value; }

    /// <summary>
    /// Gets or sets the platform name.
    /// </summary>
    [JsonPropertyName("platformName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PlatformName { get => this.platformName; set => this.platformName = value; }

    /// <summary>
    /// Gets or sets the proxy to use with this session.
    /// </summary>
    [JsonPropertyName("proxy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Proxy? Proxy { get => this.proxy; set => this.proxy = value; }

    /// <summary>
    /// Gets the dictionary containing additional capabilities to use with this session.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalCapabilities => this.additionalCapabilities;
}
