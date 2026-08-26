// <copyright file="NoneSourceActions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// Represents actions with no input device.
/// </summary>
public class NoneSourceActions : SourceActions
{
    private readonly List<INoneSourceAction> actions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="NoneSourceActions"/> class.
    /// </summary>
    public NoneSourceActions()
        : this(Guid.NewGuid().ToString())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoneSourceActions"/> class.
    /// </summary>
    /// <param name="sourceId">The unique ID of the null (pause-only) input source.</param>
    public NoneSourceActions(string sourceId)
        : base(sourceId)
    {
    }

    /// <summary>
    /// Gets the type of the source actions.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => "none";

    /// <summary>
    /// Gets the list of actions for this input device.
    /// </summary>
    [JsonPropertyName("actions")]
    public List<INoneSourceAction> Actions => this.actions;
}
