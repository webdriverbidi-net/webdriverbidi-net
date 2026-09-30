// <copyright file="SelectOption.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Script;

/// <summary>
/// An option of a <c>&lt;select&gt;</c> element to select, matched by its value, its label, or its position.
/// </summary>
public sealed class SelectOption
{
    private readonly string property;
    private readonly LocalValue match;
    private readonly string description;

    private SelectOption(string property, LocalValue match, string description)
    {
        this.property = property;
        this.match = match;
        this.description = description;
    }

    /// <summary>
    /// Matches the option whose value is exactly the given one.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The option to select.</returns>
    public static SelectOption ByValue(string value)
    {
        return new SelectOption("value", LocalValue.String(value), $"value \"{value}\"");
    }

    /// <summary>
    /// Matches the option whose label, the text it shows, is exactly the given one.
    /// </summary>
    /// <param name="label">The label.</param>
    /// <returns>The option to select.</returns>
    public static SelectOption ByLabel(string label)
    {
        return new SelectOption("label", LocalValue.String(label), $"label \"{label}\"");
    }

    /// <summary>
    /// Matches the option at the given position among the element's options, counting from 0.
    /// </summary>
    /// <param name="index">The position.</param>
    /// <returns>The option to select.</returns>
    public static SelectOption ByIndex(int index)
    {
        return new SelectOption("index", LocalValue.Number(index), $"index {index}");
    }

    /// <summary>
    /// Describes how the option is matched, such as <c>label "Red"</c>.
    /// </summary>
    /// <returns>The description.</returns>
    public override string ToString()
    {
        return this.description;
    }

    /// <summary>
    /// Converts the option to the form the page actions take.
    /// </summary>
    /// <returns>An object with the single property the option is matched by.</returns>
    internal LocalValue ToLocalValue()
    {
        return LocalValue.Object(new Dictionary<string, LocalValue>() { [this.property] = this.match });
    }
}
