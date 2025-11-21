// <copyright file="AccessibilityLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a locator for locating nodes via accessibility attributes.
/// </summary>
public class AccessibilityLocator : Locator
{
    private readonly string type = "accessibility";
    private readonly Dictionary<string, string> accessibilityAttributes = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AccessibilityLocator"/> class.
    /// </summary>
    public AccessibilityLocator()
        : base()
    {
    }

    /// <summary>
    /// Gets the type of locator.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => this.type;

    /// <summary>
    /// Gets a read-only version of a dictionary containing the accessibility attributes to use in locating nodes.
    /// </summary>
    [JsonPropertyName("value")]
    public override object Value => new ReadOnlyDictionary<string, string>(this.accessibilityAttributes);

    /// <summary>
    /// Gets or sets the accessible name to use to locate nodes.
    /// </summary>
    [JsonIgnore]
    public string? Name { get => this.GetAccessiblePropertyValue("name"); set => this.SetAccessiblePropertyValue("name", value); }

    /// <summary>
    /// Gets or sets the accessible role to use to locate nodes.
    /// </summary>
    [JsonIgnore]
    public string? Role { get => this.GetAccessiblePropertyValue("role"); set => this.SetAccessiblePropertyValue("role", value); }

    private string? GetAccessiblePropertyValue(string propertyName)
    {
        if (this.accessibilityAttributes.TryGetValue(propertyName, out string? value))
        {
            return value;
        }

        return null;
    }

    private void SetAccessiblePropertyValue(string propertyName, string? value)
    {
        if (value is not null)
        {
            this.accessibilityAttributes[propertyName] = value;
        }
        else
        {
            this.accessibilityAttributes.Remove(propertyName);
        }
    }
}
