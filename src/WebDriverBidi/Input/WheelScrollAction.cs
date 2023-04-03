// <copyright file="WheelScrollAction.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Input;

using Newtonsoft.Json;

/// <summary>
/// An action so scroll a wheel device.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class WheelScrollAction : IWheelSourceAction
{
    private readonly string actionType = "scroll";
    private ulong x = 0;
    private ulong y = 0;
    private long deltaX = 0;
    private long deltaY = 0;
    private ulong? duration;
    private Origin? origin;

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
    [JsonProperty("type")]
    public string Type => this.actionType;

    /// <summary>
    /// Gets or sets the horizontal position of the action.
    /// </summary>
    [JsonProperty("x")]
    public ulong X { get => this.x; set => this.x = value; }

    /// <summary>
    /// Gets or sets the vertical position of the action.
    /// </summary>
    [JsonProperty("y")]
    public ulong Y { get => this.y; set => this.y = value; }

    /// <summary>
    /// Gets or sets the horizontal change for the action.
    /// </summary>
    [JsonProperty("deltaX")]
    public long DeltaX { get => this.deltaX; set => this.deltaX = value; }

    /// <summary>
    /// Gets or sets the vertical change for the action.
    /// </summary>
    [JsonProperty("deltaY")]
    public long DeltaY { get => this.deltaY; set => this.deltaY = value; }

    /// <summary>
    /// Gets or sets the duration, in milliseconds, of the move.
    /// </summary>
    [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? Duration { get => this.duration; set => this.duration = value; }

    /// <summary>
    /// Gets or sets the origin of the move.
    /// </summary>
    [JsonIgnore]
    public Origin? Origin { get => this.origin; set => this.origin = value; }

    /// <summary>
    /// Gets the serializable origin of the move.
    /// </summary>
    [JsonProperty("origin", NullValueHandling = NullValueHandling.Ignore)]
    internal object? SerializableOrigin
    {
        get
        {
            if (this.origin is null)
            {
                return null;
            }

            return this.origin.Value;
        }
    }
}