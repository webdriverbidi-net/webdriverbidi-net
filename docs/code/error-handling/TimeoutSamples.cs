// <copyright file="TimeoutSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/advanced/error-handling.md - Timeout Handling

namespace WebDriverBiDi.Docs.Code.ErrorHandling;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for timeout handling documentation. Compiled at build time to prevent API drift.
/// </summary>
public class TimeoutSamples
{
    /// <summary>
    /// Preferred approach: per-command timeout override.
    /// </summary>
    public static async Task PreferredApproach(NavigateCommandParameters navParams)
    {
        #region PreferredApproach
        // Per-driver default timeout (set when creating the driver)
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        // Per-command timeout override (preferred)
        NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(
            navParams,
            TimeSpan.FromSeconds(60));  // Override for this command only

        // Quick operations can use a shorter timeout
        StatusCommandResult status = await driver.Session.StatusAsync(
            null,
            timeoutOverride: TimeSpan.FromSeconds(5));
        #endregion
    }

    /// <summary>
    /// ExecuteCommandAsync for per-command timeout.
    /// </summary>
    public static async Task ExecuteCommandTimeout(BiDiDriver driver, NavigateCommandParameters navParams)
    {
        #region ExecuteCommandAsync
        NavigateCommandResult result = await driver.ExecuteCommandAsync<NavigateCommandResult>(
            navParams,
            TimeSpan.FromSeconds(60));
        #endregion
    }

    /// <summary>
    /// Custom pattern: return null on timeout instead of throwing.
    /// </summary>
#region NavigateWithTimeoutAsync
    public async Task<NavigateCommandResult?> NavigateWithTimeoutAsync(
        BiDiDriver driver,
        NavigateCommandParameters parameters,
        TimeSpan timeout)
    {
        Task<NavigateCommandResult> navigationTask =
            driver.BrowsingContext.NavigateAsync(parameters, timeout);

        try
        {
            return await navigationTask;
        }
        catch (WebDriverBiDiTimeoutException)
        {
            Console.WriteLine($"Navigation timeout after {timeout.TotalSeconds}s");
            return null;
        }
    }
    #endregion

    /// <summary>
    /// Usage of NavigateWithTimeoutAsync.
    /// </summary>
    public async Task NavigateWithTimeoutUsage(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
        #region Usage
        // Usage
        NavigateCommandResult? result = await NavigateWithTimeoutAsync(
            driver,
            navParams,
            TimeSpan.FromSeconds(30));

        if (result == null)
        {
            Console.WriteLine("Navigation failed - timeout");
            // Handle timeout
        }
        #endregion
    }
}
