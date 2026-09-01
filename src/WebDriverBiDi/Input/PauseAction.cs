// <copyright file="PauseAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// An action to pause the execution of a device.
/// </summary>
public class PauseAction : INoneSourceAction, IKeySourceAction, IPointerSourceAction, IWheelSourceAction
{
    /// <summary>
    /// Gets the type of the action.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "pause";

    /// <summary>
    /// Gets or sets the duration of the pause.
    /// </summary>
    /// <remarks>
    /// Valid values for this property are greater than or equal to zero, as the protocol transmits the
    /// duration as an unsigned integer number of milliseconds. Unlike properties whose range the remote
    /// end enforces, a negative value is rejected by this property, because a negative duration cannot be
    /// represented on the wire.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a negative <see cref="TimeSpan"/>.</exception>
    [JsonIgnore]
    public TimeSpan? Duration
    {
        get;
        set
        {
            if (value is not null && value.Value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Duration must not be negative.");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets the duration of the pause for serialization purposes.
    /// </summary>
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal ulong? SerializableDuration
    {
        get
        {
            if (!this.Duration.HasValue)
            {
                return null;
            }

            return Convert.ToUInt64(this.Duration.Value.TotalMilliseconds);
        }
    }
}
