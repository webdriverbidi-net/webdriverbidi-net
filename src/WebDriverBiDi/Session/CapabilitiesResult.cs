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
    /// <summary>
    /// Initializes a new instance of the <see cref="CapabilitiesResult"/> class.
    /// </summary>
    [JsonConstructor]
    internal CapabilitiesResult()
    {
        this.AdditionalCapabilities = ReceivedDataDictionary.EmptyDictionary;
    }

    /// <summary>
    /// Gets a value indicating whether the browser should accept insecure (self-signed) SSL certificates.
    /// </summary>
    [JsonPropertyName("acceptInsecureCerts")]
    [JsonRequired]
    [JsonInclude]
    public bool AcceptInsecureCertificates { get; internal set; } = false;

    /// <summary>
    /// Gets the name of the browser.
    /// </summary>
    [JsonPropertyName("browserName")]
    [JsonRequired]
    [JsonInclude]
    public string BrowserName { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the version of the browser.
    /// </summary>
    [JsonPropertyName("browserVersion")]
    [JsonRequired]
    [JsonInclude]
    public string BrowserVersion { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the platform name.
    /// </summary>
    [JsonPropertyName("platformName")]
    [JsonRequired]
    [JsonInclude]
    public string PlatformName { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this session supports setting the size of the browser window.
    /// </summary>
    [JsonPropertyName("setWindowRect")]
    [JsonRequired]
    [JsonInclude]
    public bool SetWindowRect { get; internal set; } = false;

    /// <summary>
    /// Gets a value indicating the WebSocket URL used by this connection.
    /// </summary>
    [JsonPropertyName("webSocketUrl")]
    [JsonInclude]
    public string? WebSocketUrl { get; internal set; }

    /// <summary>
    /// Gets a value containing the default user agent string for this browser.
    /// </summary>
    [JsonPropertyName("userAgent")]
    [JsonRequired]
    [JsonInclude]
    public string UserAgent { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets a read-only dictionary of additional capabilities specified by this session.
    /// </summary>
    [JsonIgnore]
    public ReceivedDataDictionary AdditionalCapabilities
    {
        get
        {
            if (this.SerializableAdditionalCapabilities.Count > 0 && field.Count == 0)
            {
                field = JsonConverterUtilities.ConvertIncomingExtensionData(this.SerializableAdditionalCapabilities);
            }

            return field;
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
            if (this.SerializableUnhandledPromptBehavior is not null && field is null)
            {
                field = new UserPromptHandlerResult(this.SerializableUnhandledPromptBehavior);
            }

            return field;
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
            if (this.SerializableProxy is not null && field is null)
            {
                field = this.SerializableProxy.ProxyType switch
                {
                    ProxyType.Direct => new DirectProxyConfigurationResult((DirectProxyConfiguration)this.SerializableProxy),
                    ProxyType.System => new SystemProxyConfigurationResult((SystemProxyConfiguration)this.SerializableProxy),
                    ProxyType.AutoDetect => new AutoDetectProxyConfigurationResult((AutoDetectProxyConfiguration)this.SerializableProxy),
                    ProxyType.ProxyAutoConfig => new PacProxyConfigurationResult((PacProxyConfiguration)this.SerializableProxy),
                    ProxyType.Manual => new ManualProxyConfigurationResult((ManualProxyConfiguration)this.SerializableProxy),
                    _ => null,
                };
            }

            return field;
        }
    }

    /// <summary>
    /// Gets or sets the proxy used for this session.
    /// </summary>
    [JsonPropertyName("proxy")]
    [JsonInclude]
    internal ProxyConfiguration? SerializableProxy { get; set; }

    /// <summary>
    /// Gets or sets the behavior of the session for unhandled user prompts.
    /// </summary>
    [JsonPropertyName("unhandledPromptBehavior")]
    [JsonInclude]
    internal UserPromptHandler? SerializableUnhandledPromptBehavior { get; set; }

    /// <summary>
    /// Gets or sets the dictionary containing additional, un-enumerated capabilities for deserialization purposes.
    /// </summary>
    [JsonExtensionData]
    [JsonInclude]
    internal Dictionary<string, JsonElement> SerializableAdditionalCapabilities { get; set; } = [];
}
