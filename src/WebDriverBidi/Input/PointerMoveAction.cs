// <copyright file="PointerMoveAction.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Input;

using Newtonsoft.Json;

/// <summary>
/// An action to send a pointer move on a pointer device.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PointerMoveAction : PointerAction, IPointerSourceAction
{
    private readonly string actionType = "pointerMove";
    private ulong x = 0;
    private ulong y = 0;
    private ulong? duration;
    private Origin? origin;

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
    [JsonProperty("type")]
    public string Type => this.actionType;

    /// <summary>
    /// Gets or sets the horizontal distance of the move, measured in pixels from the origin point.
    /// </summary>
    [JsonProperty("x")]
    public ulong X { get => this.x; set => this.x = value; }

    /// <summary>
    /// Gets or sets the vertical distance of the move, measured in pixels from the origin point.
    /// </summary>
    [JsonProperty("y")]
    public ulong Y { get => this.y; set => this.y = value; }

    /// <summary>
    /// Gets or sets the duration, in milliseconds, of the move.
    /// </summary>
    [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? Duration
    { get => this.duration; set => this.duration = value; }

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