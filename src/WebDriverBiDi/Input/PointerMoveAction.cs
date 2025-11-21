// <copyright file="PointerMoveAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// An action to send a pointer move on a pointer device.
/// </summary>
public class PointerMoveAction : PointerAction, IPointerSourceAction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PointerMoveAction"/> class.
    /// </summary>
    public PointerMoveAction()
        : base()
    {
    }

    /// <summary>
    /// Gets the type of the action.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "pointerMove";

    /// <summary>
    /// Gets or sets the horizontal distance of the move, measured in pixels from the origin point.
    /// </summary>
    [JsonPropertyName("x")]
    public double X { get; set; } = 0;

    /// <summary>
    /// Gets or sets the vertical distance of the move, measured in pixels from the origin point.
    /// </summary>
    [JsonPropertyName("y")]
    public double Y { get; set; } = 0;

    /// <summary>
    /// Gets or sets the duration of the move.
    /// </summary>
    [JsonIgnore]
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Gets or sets the origin of the move.
    /// </summary>
    [JsonIgnore]
    public Origin? Origin { get; set; }

    /// <summary>
    /// Gets the duration, in milliseconds, of the move for serialization purposes.
    /// </summary>
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal ulong? SerializableDuration
    {
        get
        {
            if (this.Duration is null)
            {
                return null;
            }

            return Convert.ToUInt64(this.Duration.Value.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Gets the serializable origin of the move.
    /// </summary>
    [JsonPropertyName("origin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal object? SerializableOrigin
    {
        get
        {
            if (this.Origin is null)
            {
                return null;
            }

            return this.Origin.Value;
        }
    }
}
