// <copyright file="CapabilitiesResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing the capabilities returned by a new session.
/// </summary>
public class CapabilitiesResult
{
    private Dictionary<string, JsonElement> writableCapabilities = new();
    private bool acceptInsecureCertificates = false;
    private string browserName = string.Empty;
    private string browserVersion = string.Empty;
    private string platformName = string.Empty;
    private bool setWindowRect = false;
    private ProxyResult? proxyResult;
    private Proxy proxy = WebDriverBiDi.Session.Proxy.EmptyProxy;
    private ReceivedDataDictionary additionalCapabilities = ReceivedDataDictionary.EmptyDictionary;

    /// <summary>
    /// Gets a value indicating whether the browser should accept insecure (self-signed) SSL certificates.
    /// </summary>
    [JsonPropertyName("acceptInsecureCerts")]
    [JsonRequired]
    [JsonInclude]
    public bool AcceptInsecureCertificates { get => this.acceptInsecureCertificates; internal set => this.acceptInsecureCertificates = value; }

    /// <summary>
    /// Gets the name of the browser.
    /// </summary>
    [JsonPropertyName("browserName")]
    [JsonRequired]
    [JsonInclude]
    public string BrowserName { get => this.browserName; internal set => this.browserName = value; }

    /// <summary>
    /// Gets the version of the browser.
    /// </summary>
    [JsonPropertyName("browserVersion")]
    [JsonRequired]
    [JsonInclude]
    public string BrowserVersion { get => this.browserVersion; internal set => this.browserVersion = value; }

    /// <summary>
    /// Gets the platform name.
    /// </summary>
    [JsonPropertyName("platformName")]
    [JsonRequired]
    [JsonInclude]
    public string PlatformName { get => this.platformName; internal set => this.platformName = value; }

    /// <summary>
    /// Gets a value indicating whether this session supports setting the size of the browser window.
    /// </summary>
    [JsonPropertyName("setWindowRect")]
    [JsonRequired]
    [JsonInclude]
    public bool SetWindowRect { get => this.setWindowRect; internal set => this.setWindowRect = value; }

    /// <summary>
    /// Gets a read-only dictionary of additional capabilities specified by this session.
    /// </summary>
    [JsonIgnore]
    public ReceivedDataDictionary AdditionalCapabilities
    {
        get
        {
            if (this.writableCapabilities.Count > 0 && this.additionalCapabilities.Count == 0)
            {
                this.additionalCapabilities = JsonConverterUtilities.ConvertIncomingExtensionData(this.writableCapabilities);
            }

            return this.additionalCapabilities;
        }
    }

    /// <summary>
    /// Gets the proxy used by this session.
    /// </summary>
    [JsonIgnore]
    public ProxyResult Proxy
    {
        get
        {
            this.proxyResult ??= new ProxyResult(this.proxy);
            return this.proxyResult;
        }
    }

    /// <summary>
    /// Gets or sets the proxy used for this session.
    /// </summary>
    [JsonPropertyName("proxy")]
    [JsonRequired]
    [JsonInclude]
    internal Proxy SerializableProxy { get => this.proxy; set => this.proxy = value; }

    /// <summary>
    /// Gets or sets the dictionary containing additional, un-enumerated capabilities for deserialization purposes.
    /// </summary>
    [JsonExtensionData]
    [JsonInclude]
    internal Dictionary<string, JsonElement> SerializableAdditionalCapabilities { get => this.writableCapabilities; set => this.writableCapabilities = value; }
}