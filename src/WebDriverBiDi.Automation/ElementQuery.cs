// <copyright file="ElementQuery.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Globalization;
using System.Text;
using WebDriverBiDi.BrowsingContext;

/// <summary>
/// A lookup built by one of the GetBy helpers: the protocol locator that performs it, and a description in the
/// helper's own terms for messages.
/// </summary>
/// <param name="Locator">The protocol locator.</param>
/// <param name="Description">The description.</param>
internal sealed record ElementQuery(Locator Locator, string Description)
{
    /// <summary>
    /// Finds elements by their rendered text: containing it, ignoring case, or exactly matching it.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="exact">Whether the whole text must match, with case.</param>
    /// <returns>The query.</returns>
    public static ElementQuery ByText(string text, bool exact)
    {
        InnerTextLocator locator = new(text)
        {
            MatchType = exact ? InnerTextMatchType.Full : InnerTextMatchType.Partial,
            IgnoreCase = !exact,
        };
        return new ElementQuery(locator, Describe("getByText", text, exact));
    }

    /// <summary>
    /// Finds elements by an attribute's value: containing it, ignoring case, or exactly matching it.
    /// </summary>
    /// <param name="attributeName">The attribute, such as "placeholder".</param>
    /// <param name="helperName">The helper's name, for the description.</param>
    /// <param name="text">The text.</param>
    /// <param name="exact">Whether the whole value must match, with case.</param>
    /// <returns>The query.</returns>
    public static ElementQuery ByAttribute(string attributeName, string helperName, string text, bool exact)
    {
        // CSS matches no element for a substring match with empty text, but every value contains the empty text.
        string selector = exact
            ? $"[{attributeName}={CssString(text)}]"
            : text.Length == 0 ? $"[{attributeName}]" : $"[{attributeName}*={CssString(text)} i]";
        return new ElementQuery(new CssLocator(selector), Describe(helperName, text, exact));
    }

    /// <summary>
    /// Finds elements by their test ID attribute.
    /// </summary>
    /// <param name="attributeName">The test ID attribute's name.</param>
    /// <param name="testId">The test ID.</param>
    /// <returns>The query.</returns>
    public static ElementQuery ByTestId(string attributeName, string testId)
    {
        return new ElementQuery(new CssLocator($"[{attributeName}={CssString(testId)}]"), $"getByTestId {Quote(testId)}");
    }

    /// <summary>
    /// Finds elements by their computed accessibility role and, optionally, their exact accessible name.
    /// </summary>
    /// <param name="role">The role, such as "button".</param>
    /// <param name="name">The exact accessible name, or <see langword="null"/> for any.</param>
    /// <returns>The query.</returns>
    public static ElementQuery ByRole(string role, string? name)
    {
        AccessibilityLocator locator = new() { Role = role };
        if (name is not null)
        {
            locator.Name = name;
        }

        return new ElementQuery(locator, name is null ? $"getByRole {Quote(role)}" : $"getByRole {Quote(role)} name {Quote(name)}");
    }

    private static string Describe(string helperName, string text, bool exact)
    {
        return exact ? $"{helperName} {Quote(text)} exact" : $"{helperName} {Quote(text)}";
    }

    private static string Quote(string text)
    {
        return $"\"{text}\"";
    }

    // A CSS string: quotes and backslashes escaped, and control characters as hexadecimal escapes.
    private static string CssString(string value)
    {
        StringBuilder builder = new("\"");
        foreach (char character in value)
        {
            if (character is '"' or '\\')
            {
                builder.Append('\\').Append(character);
            }
            else if (character < ' ' || character == '\u007F')
            {
                builder.Append('\\').Append(((int)character).ToString("x", CultureInfo.InvariantCulture)).Append(' ');
            }
            else
            {
                builder.Append(character);
            }
        }

        return builder.Append('"').ToString();
    }
}
