// <copyright file="SubscribeSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/events-observables.md - Event subscription

namespace WebDriverBiDi.Docs.Code.EventsObservables;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for event subscription documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class SubscribeSamples
{
    /// <summary>
    /// Basic subscription using EventName property.
    /// </summary>
    public static async Task BasicSubscription(BiDiDriver driver)
    {
        #region BasicSubscription
        SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
            [
                driver.Log.OnEntryAdded.EventName,
                driver.Network.OnResponseCompleted.EventName,
            ]
        );

        SubscribeCommandResult result = await driver.Session.SubscribeAsync(subscribe);
        Console.WriteLine($"Subscription ID: {result.SubscriptionId}");
        #endregion
    }

    /// <summary>
    /// Single event subscription.
    /// </summary>
    public static async Task SingleEventSubscription(BiDiDriver driver)
    {
        #region SingleEventSubscription
        SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
            driver.Log.OnEntryAdded.EventName);

        await driver.Session.SubscribeAsync(subscribe);
        #endregion
    }

    /// <summary>
    /// Subscription with context filter.
    /// </summary>
    public static async Task SubscriptionWithContext(
        BiDiDriver driver,
        string contextId)
    {
        #region SubscriptionwithContext
        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName);

        // Only receive events for this specific context
        subscribe.Contexts.Add(contextId);

        await driver.Session.SubscribeAsync(subscribe);
        #endregion
    }

    /// <summary>
    /// Unsubscribe by subscription ID.
    /// </summary>
    public static async Task UnsubscribeById(BiDiDriver driver, string subscriptionId)
    {
        #region UnsubscribebyID
        // Unsubscribe by subscription ID
        UnsubscribeByIdsCommandParameters unsubscribe =
            new UnsubscribeByIdsCommandParameters();
        unsubscribe.SubscriptionIds.Add(subscriptionId);
        await driver.Session.UnsubscribeAsync(unsubscribe);
        #endregion
    }

    /// <summary>
    /// Unsubscribe by event names.
    /// </summary>
    public static async Task UnsubscribeByEventNames(BiDiDriver driver)
    {
        #region UnsubscribebyEventNames
        // Or unsubscribe by event names
        UnsubscribeByAttributesCommandParameters unsubscribe =
            new UnsubscribeByAttributesCommandParameters();
        unsubscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
        unsubscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);
        await driver.Session.UnsubscribeAsync(unsubscribe);
        #endregion
    }

    /// <summary>
    /// Subscribe to multiple events via Events.Add (alternative pattern).
    /// </summary>
    public static async Task SubscribeMultipleEvents(BiDiDriver driver)
    {
        #region SubscribeMultipleEvents
        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
        subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);
        subscribe.Events.Add(driver.BrowsingContext.OnLoad.EventName);

        SubscribeCommandResult result = await driver.Session.SubscribeAsync(subscribe);
        #endregion
    }
}
