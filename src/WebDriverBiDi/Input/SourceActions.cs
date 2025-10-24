// <copyright file="SourceActions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// Base class for input actions.
/// </summary>
[JsonDerivedType(typeof(KeySourceActions))]
[JsonDerivedType(typeof(PointerSourceActions))]
[JsonDerivedType(typeof(WheelSourceActions))]
[JsonDerivedType(typeof(NoneSourceActions))]
public abstract class SourceActions
{
    private string id = Guid.NewGuid().ToString();

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceActions"/> class.
    /// </summary>
    protected SourceActions()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceActions"/> class with a specific ID.
    /// </summary>
    /// <param name="id">The ID for this input source.</param>
    protected SourceActions(string id)
    {
        this.id = id;
    }

    /// <summary>
    /// Gets the type of the source actions.
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    /// <summary>
    /// Gets the ID of the device.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id => this.id;
}
