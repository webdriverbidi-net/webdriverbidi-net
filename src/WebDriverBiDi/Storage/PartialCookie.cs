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
    /// <summary>
    /// Initializes a new instance of the <see cref="PartialCookie"/> class.
    /// </summary>
    /// <param name="name">The name of the cookie to set.</param>
    /// <param name="value">The value of the cookie to set.</param>
    /// <param name="domain">The domain of the cookie to set.</param>
    public PartialCookie(string name, BytesValue value, string domain)
    {
        this.Name = name;
        this.Value = value;
        this.Domain = domain;
    }

    /// <summary>
    /// Gets or sets the name to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("value")]
    public BytesValue Value { get; set; } = BytesValue.Empty;

    /// <summary>
    /// Gets or sets the domain to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the byte length of the cookie when serialized in an HTTP cookie header
    /// to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Size { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is only available via HTTP headers
    /// to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("httpOnly")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HttpOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is secure, delivered via an
    /// encrypted connection like HTTPS to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("secure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Secure { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie a same site cookie to use in querying for cookies.
    /// </summary>
    [JsonPropertyName("sameSite")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CookieSameSiteValue? SameSite { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the cookie for querying cookies.
    /// </summary>
    [JsonIgnore]
    public DateTime? Expires
    {
        get
        {
            if (this.EpochExpires.HasValue)
            {
                return DateTimeUtilities.UnixEpoch.AddSeconds(this.EpochExpires.Value);
            }

            return null;
        }

        set
        {
            if (value.HasValue)
            {
                this.EpochExpires = Convert.ToUInt64((value.Value - DateTimeUtilities.UnixEpoch).TotalSeconds);
            }
            else
            {
                this.EpochExpires = null;
            }
        }
    }

    /// <summary>
    /// Gets the dictionary containing extra data to use in querying for cookies.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData { get; } = [];

    /// <summary>
    /// Gets or sets the expiration time of the cookie as the total number of milliseconds
    /// elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC) to use in
    /// querying for cookies.
    /// </summary>
    [JsonPropertyName("expiry")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal ulong? EpochExpires { get; set; }
}
