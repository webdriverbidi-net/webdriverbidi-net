// <copyright file="CapabilitiesResult.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Object containing the capabilities returned by a new session.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(CapabilitiesResultJsonConverter))]
public class CapabilitiesResult
{
    private bool acceptInsecureCertificates = false;
    private string browserName = string.Empty;
    private string browserVersion = string.Empty;
    private string platformName = string.Empty;
    private bool setWindowRect = false;
    private ProxyResult? proxyResult;
    private Proxy proxy = WebDriverBidi.Session.Proxy.EmptyProxy;
    private AdditionalCapabilities additionalCapabilities = AdditionalCapabilities.EmptyAdditionalCapabilities;

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
    public AdditionalCapabilities AdditionalCapabilities { get => this.additionalCapabilities; internal set => this.additionalCapabilities = value; }

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
}