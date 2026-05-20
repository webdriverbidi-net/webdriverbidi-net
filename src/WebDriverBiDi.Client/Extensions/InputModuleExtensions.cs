// <copyright file="InputModuleExtensions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using WebDriverBiDi.Client.Inputs;
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
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An Task representing the asynchronous operation.</returns>
    public static async Task ClickElementAsync(this InputModule module, string browsingContextId, SharedReference elementReference, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        InputBuilder inputBuilder = new();
        inputBuilder.AddClickOnElementAction(elementReference);
        PerformActionsCommandParameters parameters = new(browsingContextId);
        parameters.Actions.AddRange(inputBuilder.Build());
        await module.PerformActionsAsync(parameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sends keys to the specified element.
    /// </summary>
    /// <param name="module">The <see cref="InputModule"/> to extend.</param>
    /// <param name="browsingContextId">The ID of the browsing context containing the element to which to send keys.</param>
    /// <param name="elementReference">The <see cref="SharedReference"/> representing the element to which to send keys.</param>
    /// <param name="keysToSend">The keys to send to the element.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An Task representing the asynchronous operation.</returns>
    public static async Task SendKeysToElement(this InputModule module, string browsingContextId, SharedReference elementReference, string keysToSend, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        InputBuilder inputBuilder = new();
        inputBuilder.AddClickOnElementAction(elementReference);
        inputBuilder.AddSendKeysToActiveElementAction(keysToSend);
        PerformActionsCommandParameters parameters = new(browsingContextId);
        parameters.Actions.AddRange(inputBuilder.Build());
        await module.PerformActionsAsync(parameters, timeoutOverride, cancellationToken);
    }
}
