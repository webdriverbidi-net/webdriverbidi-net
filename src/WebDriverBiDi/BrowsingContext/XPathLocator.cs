// <copyright file="XPathLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a locator for locating nodes via XPath.
/// </summary>
public class XPathLocator : Locator
{
    private readonly string type = "xpath";
    private readonly string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="XPathLocator"/> class.
    /// </summary>
    /// <param name="value">The XPath to use in locating nodes.</param>
    public XPathLocator(string value)
        : base()
    {
        this.value = value;
    }

    /// <summary>
    /// Gets the type of locator.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => this.type;

    /// <summary>
    /// Gets the XPath to use to locate nodes for serialization purposes.
    /// </summary>
    [JsonPropertyName("value")]
    public override object Value => this.value;
}
