// <copyright file="SetCookieHeader.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// Represents a header to set a cookie in a web response.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class SetCookieHeader
{
    private string name = string.Empty;
    private BytesValue value = new(BytesValueType.String, string.Empty);
    private string? domain;
    private string? path;
    private DateTime? expires;
    private ulong? maxAge;
    private bool? isSecure;
    private bool? isHttpOnly;
    private CookieSameSiteValue? sameSite;

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
        this.name = name;
        this.value = BytesValue.FromString(value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCookieHeader"/> class.
    /// </summary>
    /// <param name="cookie">The <see cref="Cookie"/> to convert to a header.</param>
    public SetCookieHeader(Cookie cookie)
    {
        this.name = cookie.Name;
        this.value = cookie.Value;
        this.domain = cookie.Domain;
        this.path = cookie.Path;
        this.isHttpOnly = cookie.HttpOnly;
        this.isSecure = cookie.Secure;
        this.sameSite = cookie.SameSite;
        this.expires = cookie.Expires;
    }

    /// <summary>
    /// Gets or sets the name of the cookie.
    /// </summary>
    [JsonProperty("name")]
    [JsonRequired]
    public string Name { get => this.name; set => this.name = value; }

    /// <summary>
    /// Gets or sets the value of the cookie.
    /// </summary>
    [JsonProperty("value")]
    [JsonRequired]
    public BytesValue Value { get => this.value; set => this.value = value; }

    /// <summary>
    /// Gets or sets the domain of the cookie.
    /// </summary>
    [JsonProperty("domain", NullValueHandling = NullValueHandling.Ignore)]
    public string? Domain { get => this.domain; set => this.domain = value; }

    /// <summary>
    /// Gets or sets the path of the cookie.
    /// </summary>
    [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
    public string? Path { get => this.path; set => this.path = value; }

    /// <summary>
    /// Gets or sets the expiration time of the cookie.
    /// </summary>
    public DateTime? Expires { get => this.expires; set => this.expires = value; }

    /// <summary>
    /// Gets or sets the max age of the cookie.
    /// </summary>
    [JsonProperty("maxAge", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? MaxAge { get => this.maxAge; set => this.maxAge = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is secure, delivered via an
    /// encrypted connection like HTTPS.
    /// </summary>
    [JsonProperty("secure", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Secure { get => this.isSecure; set => this.isSecure = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is only available via HTTP headers
    /// (<see langword="true" />), or if the cookie can be inspected and manipulated
    /// via JavaScript (<see langword="false" />).
    /// </summary>
    [JsonProperty("httpOnly", NullValueHandling = NullValueHandling.Ignore)]
    public bool? HttpOnly { get => this.isHttpOnly; set => this.isHttpOnly = value; }

    /// <summary>
    /// Gets or sets a value detailing whether the cookie is a sames site cookie.
    /// </summary>
    [JsonProperty("sameSite", NullValueHandling = NullValueHandling.Ignore)]
    public CookieSameSiteValue? SameSite { get => this.sameSite; set => this.sameSite = value; }

    /// <summary>
    /// Gets the expiration time of the cookie as the total number of milliseconds
    /// elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonProperty("expires", NullValueHandling = NullValueHandling.Ignore)]
    internal string? EpochExpires => this.expires.HasValue ? $"{this.expires.Value.ToUniversalTime():ddd, dd MMM yyyy HH:mm:ss} GMT" : null;
}