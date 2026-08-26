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
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceActions"/> class.
    /// </summary>
    /// <param name="sourceId">The unique ID of the input source.</param>
    protected SourceActions(string sourceId)
    {
        this.Id = sourceId;
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
    public string Id { get; }
}
