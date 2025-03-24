// <copyright file="Action.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Inputs;

/// <summary>
/// Represents a user action to be taken in the browser being automated.
/// </summary>
public class Action
{
    private readonly string sourceId;
    private readonly object action;

    /// <summary>
    /// Initializes a new instance of the <see cref="Action"/> class.
    /// </summary>
    /// <param name="sourceId">The ID of the <see cref="InputSource"/> for which the action applies.</param>
    /// <param name="action">The object representing the action.</param>
    public Action(string sourceId, object action)
    {
        this.sourceId = sourceId;
        this.action = action;
    }

    /// <summary>
    /// Gets the ID of the <see cref="InputSource"/> for which this action applies.
    /// </summary>
    public string SourceId => this.sourceId;

    /// <summary>
    /// Converts this action into the appropriate type for adding to the input queue.
    /// </summary>
    /// <typeparam name="T">The type of the action to which to convert.</typeparam>
    /// <returns>The converted action.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if this action cannot be converted to the specified type.</exception>
    public T AsActionType<T>()
    {
        if (!typeof(T).IsAssignableFrom(this.action.GetType()))
        {
            throw new WebDriverBiDiException($"Object cannot be cast to type {typeof(T)}");
        }

        return (T)this.action;
    }
}
