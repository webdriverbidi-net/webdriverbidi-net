// <copyright file="Cookie.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;

/// <summary>
/// Represents a cookie in a web request or response.
/// </summary>
public class Cookie
{
    private string name = string.Empty;
    private BytesValue value = new(BytesValueType.String, string.Empty);
    private string domain = string.Empty;
    private string path = string.Empty;
    private ulong? epochExpires;
    private DateTime? expires;
    private long size = 0;
    private bool isSecure;
    private bool isHttpOnly;
    private CookieSameSiteValue sameSite = CookieSameSiteValue.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cookie"/> class.
    /// </summary>
    public Cookie()
    {
    }

    /// <summary>
    /// Gets the name of the cookie.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    [JsonInclude]
    public string Name { get => this.name; internal set => this.name = value; }

    /// <summary>
    /// Gets the value of the cookie.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonRequired]
    [JsonInclude]
    public BytesValue Value { get => this.value; internal set => this.value = value; }

    /// <summary>
    /// Gets the domain of the cookie.
    /// </summary>
    [JsonPropertyName("domain")]
    [JsonRequired]
    [JsonInclude]
    public string Domain { get => this.domain; internal set => this.domain = value; }

    /// <summary>
    /// Gets the path of the cookie.
    /// </summary>
    [JsonPropertyName("path")]
    [JsonRequired]
    [JsonInclude]
    public string Path { get => this.path; internal set => this.path = value; }

    /// <summary>
    /// Gets the expiration time of the cookie.
    /// </summary>
    [JsonIgnore]
    public DateTime? Expires => this.expires;

    /// <summary>
    /// Gets the expiration time of the cookie as the total number of milliseconds
    /// elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonPropertyName("expires")]
    [JsonInclude]
    public ulong? EpochExpires
    {
        get
        {
            return this.epochExpires;
        }

        private set
        {
            this.epochExpires = value;
            if (value.HasValue)
            {
                this.expires = DateTimeUtilities.UnixEpoch.AddMilliseconds(value.Value);
            }
        }
    }

    /// <summary>
    /// Gets the byte length of the cookie when serialized in an HTTP cookie header.
    /// </summary>
    [JsonPropertyName("size")]
    [JsonRequired]
    [JsonInclude]
    public long Size { get => this.size; internal set => this.size = value; }

    /// <summary>
    /// Gets a value indicating whether the cookie is secure, delivered via an
    /// encrypted connection like HTTPS.
    /// </summary>
    [JsonPropertyName("secure")]
    [JsonRequired]
    [JsonInclude]
    public bool Secure { get => this.isSecure; internal set => this.isSecure = value; }

    /// <summary>
    /// Gets a value indicating whether the cookie is only available via HTTP headers
    /// (<see langword="true" />), or if the cookie can be inspected and manipulated
    /// via JavaScript (<see langword="false" />).
    /// </summary>
    [JsonPropertyName("httpOnly")]
    [JsonRequired]
    [JsonInclude]
    public bool HttpOnly { get => this.isHttpOnly; internal set => this.isHttpOnly = value; }

    /// <summary>
    /// Gets a value indicating whether the cookie a same site cookie.
    /// </summary>
    [JsonPropertyName("sameSite")]
    [JsonRequired]
    [JsonInclude]
    public CookieSameSiteValue SameSite { get => this.sameSite; internal set => this.sameSite = value; }

    /// <summary>
    /// Converts this cookie to a <see cref="SetCookieHeader"/>.
    /// </summary>
    /// <returns>The SetCookieHeader representing this cookie.</returns>
    public SetCookieHeader ToSetCookieHeader()
    {
        return new SetCookieHeader(this);
    }
}