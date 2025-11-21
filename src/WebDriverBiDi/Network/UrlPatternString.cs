// <copyright file="UrlPatternString.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// The abstract base class for a value that can contain either a string or a byte array.
/// </summary>
public class UrlPatternString : UrlPattern
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UrlPatternString"/> class.
    /// </summary>
    public UrlPatternString()
        : this(string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlPatternString"/> class.
    /// </summary>
    /// <param name="pattern">The pattern to match.</param>
    public UrlPatternString(string pattern)
        : base(UrlPatternType.String)
    {
        this.Pattern = pattern;
    }

    /// <summary>
    /// Gets or sets the pattern to match.
    /// </summary>
    [JsonPropertyName("pattern")]
    [JsonInclude]
    public string Pattern { get; set; }
}
