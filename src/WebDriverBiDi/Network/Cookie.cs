// <copyright file="Cookie.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;

/// <summary>
/// Represents a cookie in a web request or response.
/// </summary>
public record Cookie
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Cookie"/> class.
    /// </summary>
    [JsonConstructor]
    private Cookie()
    {
        this.AdditionalData = ReceivedDataDictionary.EmptyDictionary;
    }

    /// <summary>
    /// Gets the name of the cookie.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    [JsonInclude]
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the value of the cookie.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonRequired]
    [JsonInclude]
    public BytesValue Value { get; private set; } = BytesValue.Empty;

    /// <summary>
    /// Gets the domain of the cookie.
    /// </summary>
    [JsonPropertyName("domain")]
    [JsonInclude]
    public string Domain { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the path of the cookie.
    /// </summary>
    [JsonPropertyName("path")]
    [JsonInclude]
    public string Path { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the expiration time of the cookie.
    /// </summary>
    [JsonIgnore]
    public DateTime? Expires { get; private set; }

    /// <summary>
    /// Gets the expiration time of the cookie as the total number of milliseconds
    /// elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonPropertyName("expiry")]
    [JsonInclude]
    public ulong? EpochExpires
    {
        get;
        private set
        {
            field = value;
            if (value.HasValue)
            {
                this.Expires = DateTimeUtilities.UnixEpoch.AddSeconds(value.Value);
            }
        }
    }

    /// <summary>
    /// Gets the byte length of the cookie when serialized in an HTTP cookie header.
    /// </summary>
    [JsonPropertyName("size")]
    [JsonInclude]
    public long Size { get; private set; } = 0;

    /// <summary>
    /// Gets a value indicating whether the cookie is secure, delivered via an
    /// encrypted connection like HTTPS.
    /// </summary>
    [JsonPropertyName("secure")]
    [JsonInclude]
    public bool Secure { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the cookie is only available via HTTP headers
    /// (<see langword="true" />), or if the cookie can be inspected and manipulated
    /// via JavaScript (<see langword="false" />).
    /// </summary>
    [JsonPropertyName("httpOnly")]
    [JsonInclude]
    public bool HttpOnly { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the cookie a same site cookie.
    /// </summary>
    [JsonPropertyName("sameSite")]
    [JsonInclude]
    public CookieSameSiteValue SameSite { get; private set; } = CookieSameSiteValue.None;

    /// <summary>
    /// Gets the read-only dictionary containing extra data returned for this cookie.
    /// </summary>
    [JsonIgnore]
    public ReceivedDataDictionary AdditionalData
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
    /// Gets additional properties deserialized with this cookie.
    /// </summary>
    [JsonExtensionData]
    [JsonInclude]
    internal Dictionary<string, JsonElement> SerializableAdditionalCapabilities { get; private set; } = [];

    /// <summary>
    /// Converts this cookie to a <see cref="SetCookieHeader"/>.
    /// </summary>
    /// <returns>The SetCookieHeader representing this cookie.</returns>
    public SetCookieHeader ToSetCookieHeader()
    {
        return new SetCookieHeader(this);
    }
}
