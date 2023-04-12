// <copyright file="InputSource.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Client;

using WebDriverBidi.Input;

/// <summary>
/// Base class for all input sources for actions.
/// </summary>
public abstract class InputSource
{
    private readonly string sourceId;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputSource"/> class.
    /// </summary>
    /// <param name="sourceId">The Id of the input source represented by this class.</param>
    protected InputSource(string sourceId)
    {
        if (string.IsNullOrEmpty(sourceId))
        {
            throw new ArgumentException("Device name must not be null or empty", nameof(sourceId));
        }

        this.sourceId = sourceId;
    }

    /// <summary>
    /// Gets the ID of this input source.
    /// </summary>
    public string SourceId
    {
        get { return this.sourceId; }
    }

    /// <summary>
    /// Gets the kind of source for this input device.
    /// </summary>
    public abstract InputSourceKind DeviceKind { get; }

    /// <summary>
    /// Creates a pause action for synchronization with other action sequences.
    /// </summary>
    /// <returns>The <see cref="Action"/> representing the action.</returns>
    public Action CreatePause()
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
    /// <returns>The <see cref="Interaction"/> representing the action.</returns>
    public Action CreatePause(TimeSpan duration)
    {
        PauseAction action = new();
        if (duration != TimeSpan.Zero)
        {
            action.Duration = duration;
        }

        return new Action(this.sourceId, action);
    }

    /// <summary>
    /// Returns a hash code for the current <see cref="InputSource"/>.
    /// </summary>
    /// <returns>A hash code for the current <see cref="InputSource"/>.</returns>
    public override int GetHashCode()
    {
        return this.sourceId.GetHashCode();
    }

    /// <summary>
    /// Returns a string that represents the current <see cref="InputSource"/>.
    /// </summary>
    /// <returns>A string that represents the current <see cref="InputSource"/>.</returns>
    public override string ToString()
    {
        return $"{this.DeviceKind} input device [name: {this.sourceId}]";
    }
}
