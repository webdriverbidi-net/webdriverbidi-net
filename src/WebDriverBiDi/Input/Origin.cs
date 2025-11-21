// <copyright file="Origin.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using WebDriverBiDi.Script;

/// <summary>
/// Represents the origin of an action.
/// </summary>
public class Origin
{
    private Origin(string originValue)
    {
        this.Value = originValue;
    }

    private Origin(ElementOrigin originValue)
    {
        this.Value = originValue;
    }

    /// <summary>
    /// Gets the action origin for the browser view port.
    /// </summary>
    public static Origin Viewport => new("viewport");

    /// <summary>
    /// Gets the action origin for the current pointer position.
    /// </summary>
    public static Origin Pointer => new("pointer");

    /// <summary>
    /// Gets the value of the action origin.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Creates an action origin using an element reference.
    /// </summary>
    /// <param name="originValue">The element origin point.</param>
    /// <returns>The action origin for the specified element reference.</returns>
    public static Origin Element(ElementOrigin originValue) => new(originValue);

    /// <summary>
    /// Creates an action origin using an element reference.
    /// </summary>
    /// <param name="elementReference">The SharedReference object containing the element reference.</param>
    /// <returns>The action origin for the specified element reference.</returns>
    public static Origin Element(SharedReference elementReference) => new(new ElementOrigin(elementReference));
}
