// <copyright file="WheelScrollAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// An action so scroll a wheel device.
/// </summary>
public class WheelScrollAction : IWheelSourceAction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WheelScrollAction"/> class.
    /// </summary>
    public WheelScrollAction()
        : base()
    {
    }

    /// <summary>
    /// Gets the type of the action.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "scroll";

    /// <summary>
    /// Gets or sets the horizontal position of the action.
    /// </summary>
    [JsonPropertyName("x")]
    public ulong X { get; set; } = 0;

    /// <summary>
    /// Gets or sets the vertical position of the action.
    /// </summary>
    [JsonPropertyName("y")]
    public ulong Y { get; set; } = 0;

    /// <summary>
    /// Gets or sets the horizontal change for the action.
    /// </summary>
    [JsonPropertyName("deltaX")]
    public long DeltaX { get; set; } = 0;

    /// <summary>
    /// Gets or sets the vertical change for the action.
    /// </summary>
    [JsonPropertyName("deltaY")]
    public long DeltaY { get; set; } = 0;

    /// <summary>
    /// Gets or sets the duration, in milliseconds, of the move.
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
