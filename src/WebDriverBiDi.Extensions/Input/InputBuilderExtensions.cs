// <copyright file="InputBuilderExtensions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using WebDriverBiDi.Script;

/// <summary>
/// Provides extension methods adding common sequences of actions to an <see cref="InputBuilder"/>.
/// </summary>
public static class InputBuilderExtensions
{
    /// <summary>
    /// Adds a click at the center of an element, with the default pointer.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="elementReference">The element.</param>
    /// <param name="button">The button to click.</param>
    /// <returns>The builder, for chaining.</returns>
    public static InputBuilder AddClickOnElementAction(this InputBuilder builder, SharedReference elementReference, PointerButton button = PointerButton.Left)
    {
        PointerInputSource pointer = builder.DefaultPointerInputSource;
        return builder.AddAction(pointer.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(elementReference))))
            .AddAction(pointer.CreatePointerDown(button))
            .AddAction(pointer.CreatePointerUp(button));
    }

    /// <summary>
    /// Adds a double click at the center of an element, with the default pointer.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="elementReference">The element.</param>
    /// <returns>The builder, for chaining.</returns>
    public static InputBuilder AddDoubleClickOnElementAction(this InputBuilder builder, SharedReference elementReference)
    {
        PointerInputSource pointer = builder.DefaultPointerInputSource;
        return builder.AddClickOnElementAction(elementReference)
            .AddAction(pointer.CreatePointerDown())
            .AddAction(pointer.CreatePointerUp());
    }

    /// <summary>
    /// Adds typing text into the element that has focus, with the default keyboard, as a press and a
    /// release of each character. A character made of several code points, such as an emoji or a
    /// letter with a combining accent, is typed as a single key.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="keysToSend">The text, which may include special keys from <see cref="Keys"/>.</param>
    /// <returns>The builder, for chaining.</returns>
    public static InputBuilder AddSendKeysToActiveElementAction(this InputBuilder builder, string keysToSend)
    {
        KeyInputSource keyboard = builder.DefaultKeyInputSource;
        foreach (string key in TextElements.Split(keysToSend))
        {
            builder.AddAction(keyboard.CreateKeyDown(key)).AddAction(keyboard.CreateKeyUp(key));
        }

        return builder;
    }

    /// <summary>
    /// Adds pressing keys together, with the default keyboard: each is pressed in order, then all are released
    /// in reverse order, as for a shortcut such as <c>AddKeyChordAction(Keys.Control, "a")</c>.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="keys">The keys, each a single character or a special key from <see cref="Keys"/>.</param>
    /// <returns>The builder, for chaining.</returns>
    public static InputBuilder AddKeyChordAction(this InputBuilder builder, params string[] keys)
    {
        KeyInputSource keyboard = builder.DefaultKeyInputSource;
        foreach (string key in keys)
        {
            builder.AddAction(keyboard.CreateKeyDown(key));
        }

        for (int index = keys.Length - 1; index >= 0; index--)
        {
            builder.AddAction(keyboard.CreateKeyUp(keys[index]));
        }

        return builder;
    }

    /// <summary>
    /// Adds dragging one element onto another, with the default pointer.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="sourceElement">The element to drag.</param>
    /// <param name="targetElement">The element on which to drop it.</param>
    /// <returns>The builder, for chaining.</returns>
    public static InputBuilder AddDragAndDropAction(this InputBuilder builder, SharedReference sourceElement, SharedReference targetElement)
    {
        PointerInputSource pointer = builder.DefaultPointerInputSource;
        return builder.AddAction(pointer.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(sourceElement))))
            .AddAction(pointer.CreatePointerDown())
            .AddAction(pointer.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(targetElement))))
            .AddAction(pointer.CreatePointerUp());
    }

    /// <summary>
    /// Adds scrolling with the default wheel, over the center of an element or over the top left of the viewport.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="deltaX">The distance to scroll horizontally, in CSS pixels.</param>
    /// <param name="deltaY">The distance to scroll vertically, in CSS pixels.</param>
    /// <param name="elementReference">The element over which to scroll, or <see langword="null"/> for the viewport.</param>
    /// <returns>The builder, for chaining.</returns>
    public static InputBuilder AddScrollAction(this InputBuilder builder, long deltaX, long deltaY, SharedReference? elementReference = null)
    {
        Origin? origin = elementReference is null ? null : Origin.Element(new ElementOrigin(elementReference));
        return builder.AddAction(builder.DefaultWheelInputSource.CreateScroll(0, 0, deltaX, deltaY, origin));
    }
}
