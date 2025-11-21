// <copyright file="UrlPatternPattern.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// The abstract base class for a value that can contain either a string or a byte array.
/// </summary>
public class UrlPatternPattern : UrlPattern
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UrlPatternPattern"/> class.
    /// </summary>
    public UrlPatternPattern()
        : base(UrlPatternType.Pattern)
    {
    }

    /// <summary>
    /// Gets or sets the protocol to match.
    /// </summary>
    [JsonPropertyName("protocol")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Protocol { get; set; }

    /// <summary>
    /// Gets or sets the host name to match.
    /// </summary>
    [JsonPropertyName("hostname")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HostName { get; set; }

    /// <summary>
    /// Gets or sets the port to match.
    /// </summary>
    [JsonPropertyName("port")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Port { get; set; }

    /// <summary>
    /// Gets or sets the path name to match.
    /// </summary>
    [JsonPropertyName("pathname")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PathName { get; set; }

    /// <summary>
    /// Gets or sets the search to match.
    /// </summary>
    [JsonPropertyName("search")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Search { get; set; }
}
