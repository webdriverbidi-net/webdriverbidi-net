// <copyright file="KeyInputSource.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using WebDriverBiDi.Input;

/// <summary>
/// A key-based input source, like a keyboard, primarily for entering text.
/// </summary>
public class KeyInputSource : InputSource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyInputSource"/> class.
    /// </summary>
    /// <param name="sourceId">The unique ID of the input source.</param>
    internal KeyInputSource(string sourceId)
        : base(sourceId)
    {
    }

    /// <summary>
    /// Gets the kind of source for this input device.
    /// </summary>
    public override InputSourceKind DeviceKind => InputSourceKind.Key;

    /// <summary>
    /// Creates a key-down action for simulating a press of a key.
    /// </summary>
    /// <param name="codePoint">The unicode character to be sent.</param>
    /// <returns>The <see cref="Action"/> representing the action.</returns>
    public Action CreateKeyDown(char codePoint)
    {
        KeyDownAction action = new(codePoint.ToString());
        return new Action(this.SourceId, action);
    }

    /// <summary>
    /// Creates a key-up action for simulating a release of a key.
    /// </summary>
    /// <param name="codePoint">The unicode character to be sent.</param>
    /// <returns>The <see cref="Action"/> representing the action.</returns>
    public Action CreateKeyUp(char codePoint)
    {
        KeyUpAction action = new(codePoint.ToString());
        return new Action(this.SourceId, action);
    }
}