// <copyright file="Keyboard.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.Input;

/// <summary>
/// A page's keyboard, typing into whatever has focus. It is one input source for the page's lifetime, so a key
/// pressed in one call stays pressed until a later call releases it, and holds for the page's other input too, such
/// as a modifier key held for a click.
/// </summary>
public sealed class Keyboard
{
    private const string KeySourceId = "automation-keyboard";
    private readonly Page page;

    /// <summary>
    /// Initializes a new instance of the <see cref="Keyboard"/> class.
    /// </summary>
    /// <param name="page">The page the keyboard belongs to.</param>
    internal Keyboard(Page page)
    {
        this.page = page;
    }

    /// <summary>
    /// Presses a key, leaving it pressed.
    /// </summary>
    /// <param name="key">The key: a special key from <see cref="Keys"/>, such as <see cref="Keys.Shift"/>, or a single character.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the key is pressed.</returns>
    public Task DownAsync(string key, CancellationToken cancellationToken = default)
    {
        KeySourceActions source = new(KeySourceId);
        source.Actions.Add(new KeyDownAction(key));
        return this.PerformAsync(source, cancellationToken);
    }

    /// <summary>
    /// Releases a key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the key is released.</returns>
    public Task UpAsync(string key, CancellationToken cancellationToken = default)
    {
        KeySourceActions source = new(KeySourceId);
        source.Actions.Add(new KeyUpAction(key));
        return this.PerformAsync(source, cancellationToken);
    }

    /// <summary>
    /// Presses and releases a key, holding down any modifier keys.
    /// </summary>
    /// <param name="key">The key: a special key from <see cref="Keys"/>, such as <see cref="Keys.Enter"/>, or a single character.</param>
    /// <param name="modifiers">The modifier keys to hold.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the keys are released.</returns>
    public Task PressAsync(string key, KeyModifiers modifiers = KeyModifiers.None, CancellationToken cancellationToken = default)
    {
        return this.PerformAsync(new InputBuilder().AddKeyChordAction([.. ModifierKeyValues.For(modifiers), key]), cancellationToken);
    }

    /// <summary>
    /// Types text, as a press and a release of each character.
    /// </summary>
    /// <param name="text">The text, which may include special keys from <see cref="Keys"/>.</param>
    /// <param name="delay">The time the browser waits between one key and the next.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the text has been typed.</returns>
    public Task TypeAsync(string text, TimeSpan delay = default, CancellationToken cancellationToken = default)
    {
        return this.PerformAsync(new InputBuilder().AddSendKeysToActiveElementAction(text, delay), cancellationToken);
    }

    // The builder splits text into characters as the protocol expects; its actions are moved to the page's own
    // keyboard, whose pressed keys persist between calls.
    private Task PerformAsync(InputBuilder builder, CancellationToken cancellationToken)
    {
        KeySourceActions source = new(KeySourceId);
        foreach (KeySourceActions built in builder.Build().OfType<KeySourceActions>())
        {
            source.Actions.AddRange(built.Actions);
        }

        return this.PerformAsync(source, cancellationToken);
    }

    private Task PerformAsync(KeySourceActions source, CancellationToken cancellationToken)
    {
        return this.page.Browser.Group.Driver.Input.PerformActionsAsync(new PerformActionsCommandParameters(this.page.Id) { Actions = { source } }, cancellationToken: cancellationToken);
    }
}
