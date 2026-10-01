// <copyright file="TextPattern.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.RegularExpressions;

/// <summary>
/// An expected string or pattern, and how an expectation describes it.
/// </summary>
internal sealed class TextPattern
{
    private static readonly Regex WhiteSpace = new(@"\s+");

    private readonly Func<string, bool> matches;

    private TextPattern(string description, Func<string, bool> matches)
    {
        this.Description = description;
        this.matches = matches;
    }

    /// <summary>
    /// Gets the description, such as <c>"Saved"</c> or <c>matching /Save/</c>.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Creates a pattern for a string.
    /// </summary>
    /// <param name="expected">The string.</param>
    /// <param name="ignoreCase">A value indicating whether case is ignored.</param>
    /// <param name="substring">A value indicating whether the string may be found anywhere in the actual one.</param>
    /// <returns>The pattern.</returns>
    public static TextPattern For(string expected, bool ignoreCase = false, bool substring = false)
    {
        StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return new TextPattern(
            ignoreCase ? $"{Quote(expected)} (ignoring case)" : Quote(expected),
            actual => substring ? actual.IndexOf(expected, comparison) >= 0 : string.Equals(actual, expected, comparison));
    }

    /// <summary>
    /// Creates a pattern for a regular expression, which may match anywhere in the actual string.
    /// </summary>
    /// <param name="expected">The regular expression.</param>
    /// <returns>The pattern.</returns>
    public static TextPattern For(Regex expected)
    {
        string flags = expected.Options.HasFlag(RegexOptions.IgnoreCase) ? "i" : string.Empty;
        return new TextPattern($"matching /{expected}/{flags}", expected.IsMatch);
    }

    /// <summary>
    /// Collapses each run of white space to one space and trims the ends.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The normalized text.</returns>
    public static string Normalize(string text)
    {
        return WhiteSpace.Replace(text, " ").Trim();
    }

    /// <summary>
    /// Quotes a string for a description.
    /// </summary>
    /// <param name="text">The string.</param>
    /// <returns>The quoted string.</returns>
    public static string Quote(string text)
    {
        return $"\"{text}\"";
    }

    /// <summary>
    /// Gets a value indicating whether an actual string matches.
    /// </summary>
    /// <param name="actual">The actual string.</param>
    /// <returns><see langword="true"/> if it matches; otherwise, <see langword="false"/>.</returns>
    public bool Matches(string actual)
    {
        return this.matches(actual);
    }
}
