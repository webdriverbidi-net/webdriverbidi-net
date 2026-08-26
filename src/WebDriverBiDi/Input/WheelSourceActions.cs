// <copyright file="WheelSourceActions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// Represents actions with a wheel input device.
/// </summary>
public class WheelSourceActions : SourceActions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WheelSourceActions"/> class.
    /// </summary>
    public WheelSourceActions()
        : this(Guid.NewGuid().ToString())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WheelSourceActions"/> class.
    /// </summary>
    /// <param name="sourceId">The unique ID of the wheel input source.</param>
    public WheelSourceActions(string sourceId)
        : base(sourceId)
    {
    }

    /// <summary>
    /// Gets the type of the source actions.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => "wheel";

    /// <summary>
    /// Gets the list of actions for this input device.
    /// </summary>
    [JsonPropertyName("actions")]
    public List<IWheelSourceAction> Actions { get; } = [];
}
