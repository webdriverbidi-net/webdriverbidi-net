// <copyright file="ExtensionsSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/advanced/webdriverbidi-extensions.md

namespace WebDriverBiDi.Docs.Code.Advanced;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;

/// <summary>
/// Snippets for the WebDriverBiDi.Extensions guide. Compiled at build time to prevent API drift.
/// </summary>
public static class ExtensionsSamples
{
    /// <summary>
    /// Handling a script that throws.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <param name="contextId">The browsing context to run the script in.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ScriptThatThrows(BiDiDriver driver, string contextId)
    {
        #region ScriptThatThrows
        try
        {
            await driver.Script.CallFunctionAsync(contextId, "() => { throw new Error('not ready'); }");
        }
        catch (ScriptException e)
        {
            // The message names the error and where it was thrown; e.Details holds the stack trace and the thrown value.
            Console.WriteLine(e.Message);
        }
        #endregion
    }

    /// <summary>
    /// Key chords, double-clicks, and drag-and-drop.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <param name="contextId">The browsing context.</param>
    /// <param name="card">An element to drag.</param>
    /// <param name="column">The element to drop it on.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task InputHelpers(BiDiDriver driver, string contextId, SharedReference card, SharedReference column)
    {
        #region InputHelpers
        InputBuilder builder = new InputBuilder()
            .AddDoubleClickOnElementAction(card)
            .AddKeyChordAction(Keys.Control, "c")
            .AddDragAndDropAction(card, column)
            .AddScrollAction(0, 400);
        await driver.Input.PerformActionsAsync(contextId, builder);
        #endregion
    }

    /// <summary>
    /// Actions of several sources in one tick.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <param name="contextId">The browsing context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task InputSimultaneousSources(BiDiDriver driver, string contextId)
    {
        #region InputSimultaneousSources
        InputBuilder builder = new();
        KeyInputSource keyboard = builder.DefaultKeyInputSource;
        PointerInputSource mouse = builder.DefaultPointerInputSource;

        // Shift is held while the mouse moves and clicks, so the click extends a selection.
        builder.AddAction(keyboard.CreateKeyDown(Keys.Shift))
            .AddActions(mouse.CreatePointerMove(250.5, 120), keyboard.CreatePause())
            .AddAction(mouse.CreatePointerDown())
            .AddAction(mouse.CreatePointerUp())
            .AddAction(keyboard.CreateKeyUp(Keys.Shift));
        await driver.Input.PerformActionsAsync(contextId, builder);
        #endregion
    }

    /// <summary>
    /// Printing captured traffic as HTTP text.
    /// </summary>
    /// <param name="traffic">Captured requests.</param>
    public static void NetworkRequestText(IReadOnlyList<NetworkRequest> traffic)
    {
        #region NetworkRequestText
        foreach (NetworkRequest request in traffic)
        {
            Console.WriteLine(request.GetRequestText());

            // Binary bodies are summarized unless asked for; Display shows the base64, Decode the bytes as UTF-8.
            Console.WriteLine(request.GetResponseText(Base64DisplayBehavior.NoDisplay));
        }
        #endregion
    }
}
