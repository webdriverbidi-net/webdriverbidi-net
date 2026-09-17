// <copyright file="UrlPatternString.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a URL pattern defined by a string.
/// </summary>
/// <remarks>
/// The string is a complete URL, not a wildcard or glob expression. The remote end parses it and compares
/// the protocol, host name, port, path and query of each request URL with those of the pattern for equality,
/// so the pattern matches only that URL, whatever its fragment. The characters <c>(</c>, <c>)</c>, <c>*</c>,
/// <c>{</c> and <c>}</c> are reserved and make the remote end reject the pattern with an invalid argument
/// error unless each is escaped with a preceding backslash (<c>\</c>). To match every URL on a host or path,
/// use <see cref="UrlPatternPattern"/> and set only the parts to compare.
/// </remarks>
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
    /// Gets or sets the pattern to match, a complete URL.
    /// </summary>
    [JsonPropertyName("pattern")]
    public string Pattern { get; set; }
}
