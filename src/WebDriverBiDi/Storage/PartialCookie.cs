// <copyright file="PartialCookie.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;
using WebDriverBiDi.Network;

/// <summary>
/// Object containing data for setting values for cookies.
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
    /// Gets or sets the name of the cookie.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the cookie.
    /// </summary>
    [JsonPropertyName("value")]
    public BytesValue Value { get; set; } = BytesValue.Empty;

    /// <summary>
    /// Gets or sets the domain of the cookie.
    /// </summary>
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path of the cookie.
    /// </summary>
    [JsonPropertyName("path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is only available via HTTP headers.
    /// </summary>
    [JsonPropertyName("httpOnly")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HttpOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is secure, delivered via an
    /// encrypted connection like HTTPS.
    /// </summary>
    [JsonPropertyName("secure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Secure { get; set; }

    /// <summary>
    /// Gets or sets the SameSite policy of the cookie.
    /// </summary>
    [JsonPropertyName("sameSite")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CookieSameSiteValue? SameSite { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the cookie.
    /// </summary>
    /// <remarks>
    /// The protocol expresses the time as whole seconds since the Unix epoch, so a value set here is truncated to
    /// the second, and a value that is not UTC is converted to UTC first; setting a value before the epoch throws.
    /// <see cref="DateTime.MaxValue"/>, the usual way to say "never expires", is therefore stored as the last whole
    /// second of 9999, and reads back as that second. The value read is always UTC.
    /// </remarks>
    [JsonIgnore]
    public DateTime? Expires
    {
        get
        {
            if (this.EpochExpires.HasValue)
            {
                return DateTimeUtilities.FromUnixEpochSeconds(this.EpochExpires.Value);
            }

            return null;
        }

        set
        {
            if (value.HasValue)
            {
                // Normalize to UTC before converting to epoch seconds; a DateTime with Kind Local
                // (or Unspecified, which ToUniversalTime treats as local) would otherwise be sent as
                // the wrong instant, offset by the local UTC offset.
                DateTime utcValue = value.Value.ToUniversalTime();
                if (utcValue < DateTimeUtilities.UnixEpoch)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Cookie expiration must be at or after the Unix epoch (1 January 1970 00:00:00 UTC).");
                }

                this.EpochExpires = DateTimeUtilities.ToUnixEpochSeconds(utcValue);
            }
            else
            {
                this.EpochExpires = null;
            }
        }
    }

    /// <summary>
    /// Gets the dictionary containing extra data for the cookie.
    /// </summary>
    /// <remarks>
    /// An entry may not reuse the name of a property this type already serializes; doing so would write that name
    /// twice in the same JSON object, and a JSON object with a duplicate name has no defined meaning. Sending a
    /// command that contains such an entry throws <see cref="WebDriverBiDiSerializationException"/> rather than
    /// emitting the ambiguous payload. Set the typed property instead.
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData { get; } = [];

    /// <summary>
    /// Gets or sets the expiration time of the cookie as the total number of seconds
    /// elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonPropertyName("expiry")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal ulong? EpochExpires { get; set; }
}
