// <copyright file="ElementOrigin.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Input;

using Newtonsoft.Json;
using WebDriverBidi.Script;

/// <summary>
/// Represents an element used as the origin for a pointer action.
/// </summary>
public class ElementOrigin
{
    private readonly string originType = "element";
    private readonly SharedReference elementReference;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementOrigin"/> class.
    /// </summary>
    /// <param name="element">The reference to the element to be used as the origin.</param>
    public ElementOrigin(SharedReference element)
    {
        this.elementReference = element;
    }

    /// <summary>
    /// Gets the type of the origin.
    /// </summary>
    [JsonProperty("type")]
    public string Type => this.originType;

    /// <summary>
    /// Gets the reference to the element used as the origin.
    /// </summary>
    [JsonProperty("element")]
    public SharedReference Element => this.elementReference;
}