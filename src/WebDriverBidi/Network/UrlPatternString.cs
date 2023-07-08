// <copyright file="UrlPatternString.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// The abstract base class for a value that can contain either a string or a byte array.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class UrlPatternString : UrlPattern
{
    private string pattern;

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
        this.pattern = pattern;
    }

    /// <summary>
    /// Gets or sets the pattern to match.
    /// </summary>
    [JsonProperty("pattern")]
    public string Pattern { get => this.pattern; set => this.pattern = value; }
}