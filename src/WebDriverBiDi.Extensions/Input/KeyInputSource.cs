// <copyright file="KeyInputSource.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

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
    /// Creates an action that presses a key.
    /// </summary>
    /// <param name="key">The key: a single character or grapheme, such as "a" or "é", or a special key from <see cref="Keys"/>.</param>
    /// <returns>The action.</returns>
    public InputAction CreateKeyDown(string key)
    {
        return new InputAction(this.SourceId, new KeyDownAction(key));
    }

    /// <summary>
    /// Creates an action that releases a key.
    /// </summary>
    /// <param name="key">The key: a single character or grapheme, such as "a" or "é", or a special key from <see cref="Keys"/>.</param>
    /// <returns>The action.</returns>
    public InputAction CreateKeyUp(string key)
    {
        return new InputAction(this.SourceId, new KeyUpAction(key));
    }
}
