// <copyright file="InputSource.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

/// <summary>
/// Base class for all input sources for actions.
/// </summary>
public abstract class InputSource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InputSource"/> class.
    /// </summary>
    /// <param name="sourceId">The Id of the input source represented by this class.</param>
    private protected InputSource(string sourceId)
    {
        this.SourceId = sourceId;
    }

    /// <summary>
    /// Gets the ID of this input source.
    /// </summary>
    public string SourceId { get; }

    /// <summary>
    /// Gets the kind of source for this input device.
    /// </summary>
    public abstract InputSourceKind DeviceKind { get; }

    /// <summary>
    /// Creates a pause action for synchronization with other action sequences.
    /// </summary>
    /// <returns>The <see cref="InputAction"/> representing the action.</returns>
    public InputAction CreatePause()
    {
        return this.CreatePause(TimeSpan.Zero);
    }

    /// <summary>
    /// Creates a pause action for synchronization with other action sequences.
    /// </summary>
    /// <param name="duration">
    /// A <see cref="TimeSpan"/> representing the duration of the pause. Note
    /// that <see cref="TimeSpan.Zero"/> pauses to synchronize with other action
    /// sequences for other input sources.
    /// </param>
    /// <returns>The <see cref="InputAction"/> representing the action.</returns>
    public InputAction CreatePause(TimeSpan duration)
    {
        PauseAction action = new();
        if (duration != TimeSpan.Zero)
        {
            action.Duration = duration;
        }

        return new InputAction(this.SourceId, action);
    }

    /// <summary>
    /// Returns a string that represents the current <see cref="InputSource"/>.
    /// </summary>
    /// <returns>A string that represents the current <see cref="InputSource"/>.</returns>
    public override string ToString()
    {
        return $"{this.DeviceKind} input device [name: {this.SourceId}]";
    }
}
