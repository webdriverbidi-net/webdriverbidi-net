// <copyright file="PartialCookie.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;
using WebDriverBiDi.Network;

/// <summary>
/// Object containing a data for setting values for cookies.
/// </summary>
public class PartialCookie
{
    private readonly Dictionary<string, object?> additionalData = new();
    private string name = string.Empty;
    private BytesValue value = BytesValue.Empty;
    private string domain = string.Empty;
    private string? path;
    private ulong? size;
    private bool? httpOnly;
    private bool? secure;
    private CookieSameSiteValue? sameSite;
    private ulong? expiry;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialCookie"/> class.
    /// </summary>
    /// <param name="name">The name of the cookie to set.</param>
    /// <param name="value">The value of the cookie to set.</param>
    /// <param name="domain">The domain of the cookie to set.</param>
    public PartialCookie(string name, BytesValue value, string domain)
    {
        this.name = name;
        this.value = value;
        this.domain = domain;
    }

    /// <summary>
    /// Gets or sets the name to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get => this.name; set => this.name = value; }

    /// <summary>
    /// Gets or sets the value to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("value")]
    public BytesValue Value { get => this.value; set => this.value = value; }

    /// <summary>
    /// Gets or sets the domain to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("domain")]
    public string Domain { get => this.domain; set => this.domain = value; }

    /// <summary>
    /// Gets or sets the path to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Path { get => this.path; set => this.path = value; }

    /// <summary>
    /// Gets or sets the byte length of the cookie when serialized in an HTTP cookie header
    /// to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Size { get => this.size; set => this.size = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is only available via HTTP headers
    /// to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("httpOnly")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HttpOnly { get => this.httpOnly; set => this.httpOnly = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is secure, delivered via an
    /// encrypted connection like HTTPS to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("secure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Secure { get => this.secure; set => this.secure = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie a same site cookie to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("sameSite")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CookieSameSiteValue? SameSite { get => this.sameSite; set => this.sameSite = value; }

    /// <summary>
    /// Gets or sets the expiration time of the cookie for querying cookies.
    /// </summary>
    [JsonIgnore]
    public DateTime? Expires
    {
        get
        {
            if (this.expiry.HasValue)
            {
                return DateTimeUtilities.UnixEpoch.AddMilliseconds(this.expiry.Value);
            }

            return null;
        }

        set
        {
            if (value.HasValue)
            {
                this.expiry = Convert.ToUInt64((value.Value - DateTimeUtilities.UnixEpoch).TotalMilliseconds);
            }
            else
            {
                this.expiry = null;
            }
        }
    }

    /// <summary>
    /// Gets the dictionary containing extra data to use in querying for cookies.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData => this.additionalData;

    /// <summary>
    /// Gets or sets the expiration time of the cookie as the total number of milliseconds
    /// elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC) to use in
    /// querying for cookies.
    /// </summary>
    [JsonPropertyName("expiry")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal ulong? EpochExpires { get => this.expiry; set => this.expiry = value; }
}
