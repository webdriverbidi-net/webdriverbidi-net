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
    private string? protocol;
    private string? hostName;
    private string? port;
    private string? pathName;
    private string? search;

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
    public string? Protocol { get => this.protocol; set => this.protocol = value; }

    /// <summary>
    /// Gets or sets the host name to match.
    /// </summary>
    [JsonPropertyName("hostname")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HostName { get => this.hostName; set => this.hostName = value; }

    /// <summary>
    /// Gets or sets the port to match.
    /// </summary>
    [JsonPropertyName("port")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Port { get => this.port; set => this.port = value; }

    /// <summary>
    /// Gets or sets the path name to match.
    /// </summary>
    [JsonPropertyName("pathname")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PathName { get => this.pathName; set => this.pathName = value; }

    /// <summary>
    /// Gets or sets the search to match.
    /// </summary>
    [JsonPropertyName("search")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Search { get => this.search; set => this.search = value; }
}