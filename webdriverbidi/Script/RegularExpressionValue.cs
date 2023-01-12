// <copyright file="RegularExpressionValue.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object representing a regular expression.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class RegularExpressionValue
{
    private string pattern;
    private string? flags;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegularExpressionValue"/> class with a given pattern.
    /// </summary>
    /// <param name="pattern">The pattern for the regular expression.</param>
    /// <param name="flags">The flags used in the regular expression.</param>
    [JsonConstructor]
    public RegularExpressionValue(string pattern)
        : this(pattern, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegularExpressionValue"/> class with a given pattern and flags.
    /// </summary>
    /// <param name="pattern">The pattern for the regular expression.</param>
    /// <param name="flags">The flags used in the regular expression.</param>
    public RegularExpressionValue(string pattern, string? flags)
    {
        this.pattern = pattern;
        this.flags = flags;
    }

    /// <summary>
    /// Gets the pattern used in the regular expression.
    /// </summary>
    [JsonProperty("pattern")]
    [JsonRequired]
    public string Pattern { get => this.pattern; internal set => this.pattern = value; }

    /// <summary>
    /// Gets the flags used in the regular expression.
    /// </summary>
    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public string? Flags { get => this.flags; internal set => this.flags = value; }

    /// <summary>
    /// Computes a hash code for this RegularExpressionValue.
    /// </summary>
    /// <returns>A hash code for the this RegularExpressionValue.</returns>
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the objects are equal; otherwise <see langword="false" />.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is not RegularExpressionValue other)
        {
            return false;
        }

        bool areEqual = this.pattern == other.pattern;
        if (this.flags is null && other.flags is null)
        {
            return areEqual;
        }

        return areEqual && this.flags == other.flags;
    }
}