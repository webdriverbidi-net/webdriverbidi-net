// <copyright file="CapabilitiesResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;

/// <summary>
/// Object containing the capabilities returned by a new session.
/// </summary>
public record CapabilitiesResult
{
    private Dictionary<string, JsonElement> writableCapabilities = new();
    private bool acceptInsecureCertificates = false;
    private string browserName = string.Empty;
    private string browserVersion = string.Empty;
    private string platformName = string.Empty;
    private bool setWindowRect = false;
    private string userAgent = string.Empty;
    private UserPromptHandlerResult? unhandledPromptHandlerResult;
    private ProxyConfigurationResult? proxyResult;
    private string? webSocketUrl;
    private ProxyConfiguration? proxy;
    private UserPromptHandler? unhandledPromptBehavior;
    private ReceivedDataDictionary additionalCapabilities = ReceivedDataDictionary.EmptyDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapabilitiesResult"/> class.
    /// </summary>
    [JsonConstructor]
    internal CapabilitiesResult()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the browser should accept insecure (self-signed) SSL certificates.
    /// </summary>
    [JsonPropertyName("acceptInsecureCerts")]
    [JsonRequired]
    [JsonInclude]
    public bool AcceptInsecureCertificates { get => this.acceptInsecureCertificates; private set => this.acceptInsecureCertificates = value; }

    /// <summary>
    /// Gets the name of the browser.
    /// </summary>
    [JsonPropertyName("browserName")]
    [JsonRequired]
    [JsonInclude]
    public string BrowserName { get => this.browserName; private set => this.browserName = value; }

    /// <summary>
    /// Gets the version of the browser.
    /// </summary>
    [JsonPropertyName("browserVersion")]
    [JsonRequired]
    [JsonInclude]
    public string BrowserVersion { get => this.browserVersion; private set => this.browserVersion = value; }

    /// <summary>
    /// Gets the platform name.
    /// </summary>
    [JsonPropertyName("platformName")]
    [JsonRequired]
    [JsonInclude]
    public string PlatformName { get => this.platformName; private set => this.platformName = value; }

    /// <summary>
    /// Gets a value indicating whether this session supports setting the size of the browser window.
    /// </summary>
    [JsonPropertyName("setWindowRect")]
    // TODO (Issue #18): Uncomment the JsonRequired attribute once https://bugzilla.mozilla.org/show_bug.cgi?id=1916522 is fixed.
    // [JsonRequired]
    [JsonInclude]
    public bool SetWindowRect { get => this.setWindowRect; private set => this.setWindowRect = value; }

    /// <summary>
    /// Gets a value indicating the WebSocket URL used by this connection.
    /// </summary>
    [JsonPropertyName("webSocketUrl")]
    [JsonInclude]
    public string? WebSocketUrl { get => this.webSocketUrl; private set => this.webSocketUrl = value; }

    /// <summary>
    /// Gets a value containing the default user agent string for this browser.
    /// </summary>
    [JsonPropertyName("userAgent")]
    [JsonRequired]
    [JsonInclude]
    public string UserAgent { get => this.userAgent; private set => this.userAgent = value; }

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
    /// Gets the behavior of the session for unhandled user prompts.
    /// </summary>
    [JsonIgnore]
    public UserPromptHandlerResult? UnhandledPromptBehavior
    {
        get
        {
            if (this.unhandledPromptBehavior is not null && this.unhandledPromptHandlerResult is null)
            {
                this.unhandledPromptHandlerResult = new UserPromptHandlerResult(this.unhandledPromptBehavior);
            }

            return this.unhandledPromptHandlerResult;
        }
    }

    /// <summary>
    /// Gets the proxy used by this session.
    /// </summary>
    [JsonIgnore]
    public ProxyConfigurationResult? Proxy
    {
        get
        {
            if (this.proxy is not null && this.proxyResult is null)
            {
                switch (this.proxy.ProxyType)
                {
                    case ProxyType.Direct:
                        this.proxyResult ??= new DirectProxyConfigurationResult((DirectProxyConfiguration)this.proxy);
                        break;
                    case ProxyType.System:
                        this.proxyResult ??= new SystemProxyConfigurationResult((SystemProxyConfiguration)this.proxy);
                        break;
                    case ProxyType.AutoDetect:
                        this.proxyResult ??= new AutoDetectProxyConfigurationResult((AutoDetectProxyConfiguration)this.proxy);
                        break;
                    case ProxyType.ProxyAutoConfig:
                        this.proxyResult ??= new PacProxyConfigurationResult((PacProxyConfiguration)this.proxy);
                        break;
                    case ProxyType.Manual:
                        this.proxyResult ??= new ManualProxyConfigurationResult((ManualProxyConfiguration)this.proxy);
                        break;
                }
            }

            return this.proxyResult;
        }
    }

    /// <summary>
    /// Gets or sets the proxy used for this session.
    /// </summary>
    [JsonPropertyName("proxy")]
    [JsonInclude]
    internal ProxyConfiguration? SerializableProxy { get => this.proxy; set => this.proxy = value; }

    /// <summary>
    /// Gets or sets the behavior of the session for unhandled user prompts.
    /// </summary>
    [JsonPropertyName("unhandledPromptBehavior")]
    [JsonInclude]
    internal UserPromptHandler? SerializableUnhandledPromptBehavior { get => this.unhandledPromptBehavior; set => this.unhandledPromptBehavior = value; }

    /// <summary>
    /// Gets or sets the dictionary containing additional, un-enumerated capabilities for deserialization purposes.
    /// </summary>
    [JsonExtensionData]
    [JsonInclude]
    internal Dictionary<string, JsonElement> SerializableAdditionalCapabilities { get => this.writableCapabilities; set => this.writableCapabilities = value; }
}
