// <copyright file="CapabilitiesResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing the capabilities returned by a new session.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CapabilitiesResult
{
    private readonly Dictionary<string, object?> writableCapabilities = new();
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
    [JsonProperty("acceptInsecureCerts")]
    [JsonRequired]
    public bool AcceptInsecureCertificates { get => this.acceptInsecureCertificates; internal set => this.acceptInsecureCertificates = value; }

    /// <summary>
    /// Gets the name of the browser.
    /// </summary>
    [JsonProperty("browserName")]
    [JsonRequired]
    public string BrowserName { get => this.browserName; internal set => this.browserName = value; }

    /// <summary>
    /// Gets the version of the browser.
    /// </summary>
    [JsonProperty("browserVersion")]
    [JsonRequired]
    public string BrowserVersion { get => this.browserVersion; internal set => this.browserVersion = value; }

    /// <summary>
    /// Gets the platform name.
    /// </summary>
    [JsonProperty("platformName")]
    [JsonRequired]
    public string PlatformName { get => this.platformName; internal set => this.platformName = value; }

    /// <summary>
    /// Gets a value indicating whether this session supports setting the size of the browser window.
    /// </summary>
    [JsonProperty("setWindowRect")]
    [JsonRequired]
    public bool SetWindowRect { get => this.setWindowRect; internal set => this.setWindowRect = value; }

    /// <summary>
    /// Gets a read-only dictionary of additional capabilities specified by this session.
    /// </summary>
    public ReceivedDataDictionary AdditionalCapabilities
    {
        get
        {
            if (this.writableCapabilities.Count > 0 && this.additionalCapabilities.Count == 0)
            {
                this.additionalCapabilities = new ReceivedDataDictionary(this.writableCapabilities);
            }

            return this.additionalCapabilities;
        }
    }

    /// <summary>
    /// Gets the proxy used by this session.
    /// </summary>
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
    [JsonProperty("proxy")]
    [JsonRequired]
    internal Proxy SerializableProxy { get => this.proxy; set => this.proxy = value; }

    /// <summary>
    /// Gets the dictionary containing additional, un-enumerated capabilities for deserialization purposes.
    /// </summary>
    [JsonExtensionData]
    [JsonConverter(typeof(ReceivedDataJsonConverter))]
    internal Dictionary<string, object?> SerializableAdditionalCapabilities => this.writableCapabilities;
}
