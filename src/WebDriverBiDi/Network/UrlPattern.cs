// <copyright file="UrlPattern.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// The abstract base class for a value that can contain either a string or a byte array.
/// </summary>
[JsonDerivedType(typeof(UrlPatternPattern))]
[JsonDerivedType(typeof(UrlPatternString))]
public class UrlPattern
{
    private readonly UrlPatternType patternType;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlPattern"/> class.
    /// </summary>
    /// <param name="patternType">The type of pattern to create.</param>
    protected UrlPattern(UrlPatternType patternType)
    {
        this.patternType = patternType;
    }

    /// <summary>
    /// Gets the type of this UrlPattern.
    /// </summary>
    [JsonPropertyName("type")]
    public UrlPatternType Type => this.patternType;
}