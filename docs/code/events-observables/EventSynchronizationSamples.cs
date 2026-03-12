// <copyright file="EventSynchronizationSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/events-observables.md - Event synchronization

namespace WebDriverBiDi.Docs.Code.EventsObservables;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;

/// <summary>
/// Snippets for event synchronization documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class EventSynchronizationSamples
{
    /// <summary>
    /// Waiting for a single event with checkpoint.
    /// </summary>
    public static async Task WaitForSingleEvent(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region WaitforSingleEvent
        EventObserver<NavigationEventArgs> observer = 
            driver.BrowsingContext.OnLoad.AddObserver((e) =>
            {
                Console.WriteLine($"Loaded: {e.Url}");
            });

        // Set checkpoint for 1 event (default)
        observer.SetCheckpoint();

        // Trigger navigation
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait up to 10 seconds for the event
        bool eventOccurred = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));

        if (eventOccurred)
        {
            Console.WriteLine("Page loaded!");
        }
        else
        {
            Console.WriteLine("Timeout waiting for page load");
        }
#endregion
    }

    /// <summary>
    /// Waiting for multiple events with checkpoint.
    /// </summary>
    public static async Task WaitForMultipleEvents(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region WaitforMultipleEvents
        EventObserver<ResponseCompletedEventArgs> observer = 
            driver.Network.OnResponseCompleted.AddObserver((e) =>
            {
                Console.WriteLine($"Response: {e.Response.Url}");
            });

        // Wait for 5 network responses
        observer.SetCheckpoint(5);

        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait for all 5 responses
        bool allReceived = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));
        Console.WriteLine($"Received all 5 responses: {allReceived}");
#endregion
    }
}
