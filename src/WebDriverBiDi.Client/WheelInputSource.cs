// <copyright file="WheelInputSource.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using WebDriverBiDi.Input;

/// <summary>
/// A wheel input source, like a mouse wheel, for primarily for providing discrete consecutive input values.
/// </summary>
public class WheelInputSource : InputSource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WheelInputSource"/> class.
    /// </summary>
    /// <param name="sourceId">The unique ID of the input source.</param>
    internal WheelInputSource(string sourceId)
        : base(sourceId)
    {
    }

    /// <summary>
    /// Gets the kind of source for this input device.
    /// </summary>
    public override InputSourceKind DeviceKind => InputSourceKind.Wheel;

    /// <summary>
    /// Creates a wheel scroll action, simulating the scroll of the wheel.
    /// </summary>
    /// <param name="x">The horizontal distance of the scroll, measured in pixels from the origin point.</param>
    /// <param name="y">The vertical distance of the scroll, measured in pixels from the origin point.</param>
    /// <param name="deltaX">The horizontal distance of each increment of the scroll, measured in pixels.</param>
    /// <param name="deltaY">The vertical distance of each increment of the scroll, measured in pixels.</param>
    /// <param name="origin">Optional origin point for the wheel scroll. Defaults to <see langword="null"/>, which implies the origin is the browser view port.</param>
    /// <param name="duration">Optional duration for the wheel scroll. Defaults to <see langword="null"/>, which implies a zero duration.</param>
    /// <returns>The <see cref="Action"/> representing the action.</returns>
    public Action CreateScroll(ulong x, ulong y, long deltaX, long deltaY, Origin? origin = null, TimeSpan? duration = null)
    {
        WheelScrollAction action = new()
        {
            X = x,
            Y = y,
            DeltaX = deltaX,
            DeltaY = deltaY,
            Duration = duration,
            Origin = origin,
        };
        return new Action(this.SourceId, action);
    }
}