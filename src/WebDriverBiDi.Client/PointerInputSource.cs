// <copyright file="PointerInputSource.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using WebDriverBiDi.Input;

/// <summary>
/// A pointer input source, like a mouse, pen, or stylus. Also used for touch actions.
/// </summary>
public class PointerInputSource : InputSource
{
    private readonly PointerType pointerType;

    /// <summary>
    /// Initializes a new instance of the <see cref="PointerInputSource"/> class.
    /// </summary>
    /// <param name="sourceId">The unique ID of the input source.</param>
    /// <param name="pointerType">The type of pointer to create.</param>
    internal PointerInputSource(string sourceId, PointerType pointerType)
        : base(sourceId)
    {
        this.pointerType = pointerType;
    }

    /// <summary>
    /// Gets the kind of source for this input device.
    /// </summary>
    public override InputSourceKind DeviceKind => InputSourceKind.Pointer;

    /// <summary>
    /// Creates a pointer down action for simulating the press of a pointer button.
    /// </summary>
    /// <param name="button">The button to simulate the press of. Defaults to the "left" (primary) button.</param>
    /// <param name="additionalProperties">Optional additional properties for the pointer down action. Defaults to <see langword="null"/>.</param>
    /// <returns>The <see cref="Action"/> representing the action.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the pointer type is a touch pointer, and the button is other than the "left" button.
    /// </exception>
    public Action CreatePointerDown(PointerButton button = PointerButton.Left, PointerActionProperties? additionalProperties = null)
    {
        if (this.pointerType == PointerType.Touch && button != PointerButton.Left)
        {
            throw new ArgumentException($"Button for pointer down actions of touch pointer types must be PointerButton.Left", nameof(button));
        }

        PointerDownAction action = new((long)button);
        if (additionalProperties is not null)
        {
            action.Width = additionalProperties.Width;
            action.Height = additionalProperties.Height;
            action.Pressure = additionalProperties.Pressure;
            action.TangentialPressure = additionalProperties.TangentialPressure;
            action.Twist = additionalProperties.Twist;
            action.AltitudeAngle = additionalProperties.AltitudeAngle;
            action.AzimuthAngle = additionalProperties.AzimuthAngle;
        }

        return new Action(this.SourceId, action);
    }

    /// <summary>
    /// Creates a pointer up action for simulating the press of a pointer button.
    /// </summary>
    /// <param name="button">The button to simulate the press of. Defaults to the "left" (primary) button.</param>
    /// <param name="additionalProperties">Optional additional properties for the pointer up action. Defaults to <see langword="null"/>.</param>
    /// <returns>The <see cref="Action"/> representing the action.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the pointer type is a touch pointer, and the button is other than the "left" button.
    /// </exception>
    public Action CreatePointerUp(PointerButton button = PointerButton.Left, PointerActionProperties? additionalProperties = null)
    {
        if (this.pointerType == PointerType.Touch && button != PointerButton.Left)
        {
            throw new ArgumentException($"Button for pointer up actions of touch pointer types must be PointerButton.Left", nameof(button));
        }

        PointerUpAction action = new((long)button);
        if (additionalProperties is not null)
        {
            action.Width = additionalProperties.Width;
            action.Height = additionalProperties.Height;
            action.Pressure = additionalProperties.Pressure;
            action.TangentialPressure = additionalProperties.TangentialPressure;
            action.Twist = additionalProperties.Twist;
            action.AltitudeAngle = additionalProperties.AltitudeAngle;
            action.AzimuthAngle = additionalProperties.AzimuthAngle;
        }

        return new Action(this.SourceId, action);
    }

    /// <summary>
    /// Creates a pointer move action, simulating the movement of a pointer.
    /// </summary>
    /// <param name="x">The horizontal distance of the move, measured in pixels from the origin point.</param>
    /// <param name="y">The vertical distance of the move, measured in pixels from the origin point.</param>
    /// <param name="origin">Optional origin point for the pointer move. Defaults to <see langword="null"/>, which implies the origin is the browser view port.</param>
    /// <param name="duration">Optional duration for the pointer move. Defaults to <see langword="null"/>, which implies a zero duration.</param>
    /// <param name="additionalProperties">Optional additional properties for the pointer move action. Defaults to <see langword="null"/>.</param>
    /// <returns>The <see cref="Action"/> representing the action.</returns>
    public Action CreatePointerMove(long x, long y, Origin? origin = null, TimeSpan? duration = null, PointerActionProperties? additionalProperties = null)
    {
        PointerMoveAction action = new()
        {
            X = x,
            Y = y,
            Duration = duration,
            Origin = origin,
        };
        if (additionalProperties is not null)
        {
            action.Width = additionalProperties.Width;
            action.Height = additionalProperties.Height;
            action.Pressure = additionalProperties.Pressure;
            action.TangentialPressure = additionalProperties.TangentialPressure;
            action.Twist = additionalProperties.Twist;
            action.AltitudeAngle = additionalProperties.AltitudeAngle;
            action.AzimuthAngle = additionalProperties.AzimuthAngle;
        }

        return new Action(this.SourceId, action);
    }
}
