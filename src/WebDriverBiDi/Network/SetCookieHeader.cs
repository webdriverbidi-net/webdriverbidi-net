// <copyright file="SetCookieHeader.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a header to set a cookie in a web response.
/// </summary>
public class SetCookieHeader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetCookieHeader"/> class.
    /// </summary>
    public SetCookieHeader()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCookieHeader"/> class.
    /// </summary>
    /// <param name="name">The name of the cookie.</param>
    /// <param name="value">The string value of the cookie.</param>
    public SetCookieHeader(string name, string value)
    {
        this.Name = name;
        this.Value = BytesValue.FromString(value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCookieHeader"/> class.
    /// </summary>
    /// <param name="cookie">The <see cref="Cookie"/> to convert to a header.</param>
    public SetCookieHeader(Cookie cookie)
    {
        this.Name = cookie.Name;
        this.Value = cookie.Value;
        this.Domain = cookie.Domain;
        this.Path = cookie.Path;
        this.HttpOnly = cookie.HttpOnly;
        this.Secure = cookie.Secure;
        this.SameSite = cookie.SameSite;
        this.Expires = cookie.Expires;
    }

    /// <summary>
    /// Gets or sets the name of the cookie.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the cookie.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonRequired]
    public BytesValue Value { get; set; } = BytesValue.Empty;

    /// <summary>
    /// Gets or sets the domain of the cookie.
    /// </summary>
    [JsonPropertyName("domain")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Domain { get; set; }

    /// <summary>
    /// Gets or sets the path of the cookie.
    /// </summary>
    [JsonPropertyName("path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the cookie.
    /// </summary>
    [JsonIgnore]
    public DateTime? Expires { get; set; }

    /// <summary>
    /// Gets or sets the max age of the cookie.
    /// </summary>
    [JsonPropertyName("maxAge")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? MaxAge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is secure, delivered via an
    /// encrypted connection like HTTPS.
    /// </summary>
    [JsonPropertyName("secure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Secure { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is only available via HTTP headers
    /// (<see langword="true" />), or if the cookie can be inspected and manipulated
    /// via JavaScript (<see langword="false" />).
    /// </summary>
    [JsonPropertyName("httpOnly")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HttpOnly { get; set; }

    /// <summary>
    /// Gets or sets a value detailing whether the cookie is a sames site cookie.
    /// </summary>
    [JsonPropertyName("sameSite")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CookieSameSiteValue? SameSite { get; set; }

    /// <summary>
    /// Gets the expiration time of the cookie as the total number of milliseconds
    /// elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonPropertyName("expiry")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal string? EpochExpires => this.Expires.HasValue ? $"{this.Expires.Value.ToUniversalTime():ddd, dd MMM yyyy HH:mm:ss} GMT" : null;
}
