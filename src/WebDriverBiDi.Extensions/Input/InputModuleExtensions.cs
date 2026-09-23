// <copyright file="InputModuleExtensions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using WebDriverBiDi.Script;

/// <summary>
/// Provides extension methods for the Input module.
/// </summary>
public static class InputModuleExtensions
{
    /// <summary>
    /// Clicks on the specified element.
    /// </summary>
    /// <param name="module">The <see cref="InputModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context containing the element to click.</param>
    /// <param name="elementReference">The <see cref="SharedReference"/> representing the element to be clicked.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ClickElementAsync(this InputModule module, string browsingContextId, SharedReference elementReference, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        await module.PerformActionsAsync(browsingContextId, new InputBuilder().AddClickOnElementAction(elementReference), timeoutOverride, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Types text into the element that has focus, as a sequence of key presses. To type into a
    /// particular element, focus it first, for example with <see cref="ClickElementAsync"/>.
    /// </summary>
    /// <param name="module">The <see cref="InputModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context.</param>
    /// <param name="keysToSend">The text to type, which may include special keys from <see cref="Keys"/>.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SendKeysAsync(this InputModule module, string browsingContextId, string keysToSend, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        await module.PerformActionsAsync(browsingContextId, new InputBuilder().AddSendKeysToActiveElementAction(keysToSend), timeoutOverride, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Performs the actions an <see cref="InputBuilder"/> has built.
    /// </summary>
    /// <param name="module">The <see cref="InputModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context.</param>
    /// <param name="builder">The builder.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task PerformActionsAsync(this InputModule module, string browsingContextId, InputBuilder builder, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        PerformActionsCommandParameters parameters = new(browsingContextId);
        parameters.Actions.AddRange(builder.Build());
        await module.PerformActionsAsync(parameters, timeoutOverride, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Releases every key and button still pressed by earlier actions, and resets the input state.
    /// </summary>
    /// <param name="module">The <see cref="InputModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ReleaseActionsAsync(this InputModule module, string browsingContextId, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        await module.ReleaseActionsAsync(new ReleaseActionsCommandParameters(browsingContextId), timeoutOverride, cancellationToken).ConfigureAwait(false);
    }
}
