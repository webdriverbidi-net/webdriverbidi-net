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
    /// Subscribes to events, in every browsing context or in the given ones.
    /// </summary>
    /// <param name="module">The <see cref="SessionModule"/> to extend.</param>
    /// <param name="eventNames">The names of the events, such as "log.entryAdded".</param>
    /// <param name="browsingContextIds">The browsing contexts in which to subscribe, or <see langword="null"/> for all.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
    /// <returns>The ID of the subscription, for <see cref="UnsubscribeAsync"/>.</returns>
    public static async Task<string> SubscribeAsync(this SessionModule module, IEnumerable<string> eventNames, IEnumerable<string>? browsingContextIds = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        SubscribeCommandParameters parameters = new([.. eventNames], browsingContextIds?.ToList());
        SubscribeCommandResult result = await module.SubscribeAsync(parameters, timeoutOverride, cancellationToken).ConfigureAwait(false);
        return result.SubscriptionId;
    }

    /// <summary>
    /// Removes a subscription.
    /// </summary>
    /// <param name="module">The <see cref="SessionModule"/> to extend.</param>
    /// <param name="subscriptionId">The ID of the subscription.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A token that cancels waiting for the command's response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task UnsubscribeAsync(this SessionModule module, string subscriptionId, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        await module.UnsubscribeAsync(new UnsubscribeByIdsCommandParameters(subscriptionId), timeoutOverride, cancellationToken).ConfigureAwait(false);
    }
}
