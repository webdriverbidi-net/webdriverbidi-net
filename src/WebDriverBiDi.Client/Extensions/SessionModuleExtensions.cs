// <copyright file="SessionModuleExtensions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// Provides extension methods for the Session module.
/// </summary>
public static class SessionModuleExtensions
{
    /// <summary>
    /// Subscribes to one or more events globally.
    /// </summary>
    /// <param name="module">The <see cref="SessionModule"/> to extend.</param>
    /// <param name="eventNames">A list of the names of the events to which to subscribe.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An Task representing the asynchronous operation.</returns>
    public static async Task<string> SubscribeAsync(this SessionModule module, List<string> eventNames, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        SubscribeCommandParameters parameters = new(eventNames);
        SubscribeCommandResult result = await module.SubscribeAsync(parameters).ConfigureAwait(false);
        return result.SubscriptionId;
    }

    /// <summary>
    /// Unsubscribes to one or more event subscriptions.
    /// </summary>
    /// <param name="module">The <see cref="SessionModule"/> to extend.</param>
    /// <param name="subscriptionId">The ID of the subscription containing the events from which to unsubscribe.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An Task representing the asynchronous operation.</returns>
    public static async Task UnsubscribeAsync(this SessionModule module, string subscriptionId, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        UnsubscribeByIdsCommandParameters parameters = new();
        parameters.SubscriptionIds.Add(subscriptionId);
        await module.UnsubscribeAsync(parameters).ConfigureAwait(false);
    }
}
