// <copyright file="InputBuilderExtensions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Inputs;

using WebDriverBiDi.Input;
using WebDriverBiDi.Script;

/// <summary>
/// Provides extension methods for the <see cref="InputBuilder"/> class for adding input sequences for execution in the browser.
/// </summary>
public static class InputBuilderExtensions
{
    /// <summary>
    /// Adds an action to click on a specified element.
    /// </summary>
    /// <param name="builder">The <see cref="InputBuilder"/> used to create proper payloads for action types.</param>
    /// <param name="elementReference">The <see cref="SharedReference"/> representing the element to click on.</param>
    public static void AddClickOnElementAction(this InputBuilder builder, SharedReference elementReference)
    {
        builder.AddAction(builder.DefaultPointerInputSource.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(elementReference))))
            .AddAction(builder.DefaultPointerInputSource.CreatePointerDown(PointerButton.Left))
            .AddAction(builder.DefaultPointerInputSource.CreatePointerUp());
    }

    /// <summary>
    /// Adds an action to send a series of keystrokes to the actively focused element.
    /// </summary>
    /// <param name="builder">The <see cref="InputBuilder"/> used to create proper payloads for action types.</param>
    /// <param name="keysToSend">The keys to send to the actively focused element.</param>
    public static void AddSendKeysToActiveElementAction(this InputBuilder builder, string keysToSend)
    {
        foreach (char character in keysToSend)
        {
            builder.AddAction(builder.DefaultKeyInputSource.CreateKeyDown(character))
                .AddAction(builder.DefaultKeyInputSource.CreateKeyUp(character));
        }
    }
}
