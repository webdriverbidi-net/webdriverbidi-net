// <copyright file="SpeculationModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/speculation.md

#pragma warning disable CS8600, CS8602, CS1591

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Session;
using WebDriverBiDi.Speculation;

/// <summary>
/// Snippets for Speculation module documentation. Compiled at build time to prevent API drift.
/// The WebDriver BiDi spec defines only speculation.prefetchStatusUpdated; there are no
/// commands for adding or removing speculation rules.
/// </summary>
public static class SpeculationModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
        #region AccessingModule
        SpeculationModule speculation = driver.Speculation;
        #endregion
    }

    /// <summary>
    /// Subscribe to prefetch status updates.
    /// </summary>
    public static async Task SubscribeToPrefetchStatus(BiDiDriver driver)
    {
        #region SubscribetoPrefetchStatus
        driver.Speculation.OnPrefetchStatusUpdated.AddObserver((e) =>
        {
            Console.WriteLine($"Prefetch {e.Url}: {e.Status}");
        });

        SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
            driver.Speculation.OnPrefetchStatusUpdated.EventName);
        await driver.Session.SubscribeAsync(subscribe);
        #endregion
    }

    /// <summary>
    /// Monitor prefetch status while navigating to a page that triggers prefetches.
    /// </summary>
    public static async Task MonitorPrefetchDuringNavigation(BiDiDriver driver, string contextId)
    {
        #region MonitorPrefetchDuringNavigation
        var collected = new List<PrefetchStatusUpdatedEventArgs>();

        driver.Speculation.OnPrefetchStatusUpdated.AddObserver((e) =>
        {
            collected.Add(e);
        });

        var subscribe = new SubscribeCommandParameters(
            driver.Speculation.OnPrefetchStatusUpdated.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com")
            { Wait = ReadinessState.Complete });

        await Task.Delay(2000);

        foreach (var evt in collected)
        {
            Console.WriteLine($"{evt.Url} -> {evt.Status}");
        }
        #endregion
    }
}
