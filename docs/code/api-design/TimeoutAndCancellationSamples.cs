// <copyright file="TimeoutAndCancellationSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/advanced/api-design.md

namespace WebDriverBiDi.Docs.Code.ApiDesign;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;
using WebDriverBiDi.UserAgentClientHints;

/// <summary>
/// Snippets for API design timeout and cancellation documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class TimeoutAndCancellationSamples
{
    /// <summary>
    /// Optional parameters - commands with no required properties.
    /// </summary>
    public static async Task OptionalParameters(BiDiDriver driver)
    {
        #region OptionalParameters
        // All equivalent—parameters optional
        GetTreeCommandResult tree1 = await driver.BrowsingContext.GetTreeAsync(null);
        GetTreeCommandResult tree2 = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
        StatusCommandResult status = await driver.Session.StatusAsync(null);
        GetCookiesCommandResult cookies = await driver.Storage.GetCookiesAsync(null);
        #endregion
    }

    /// <summary>
    /// Required parameters - commands with reset property.
    /// </summary>
    public static async Task RequiredParameters(BiDiDriver driver)
    {
        #region RequiredParameters
        // ✅ Explicit reset
        await driver.UserAgentClientHints.SetClientHintsOverrideAsync(
            SetClientHintsOverrideCommandParameters.ResetClientHintsOverride);

        // ✅ Explicit set
        SetClientHintsOverrideCommandParameters setParams = new SetClientHintsOverrideCommandParameters();
        setParams.ClientHints = new ClientHintsMetadata { /* ... */ };
        await driver.UserAgentClientHints.SetClientHintsOverrideAsync(setParams);
        #endregion
    }

    /// <summary>
    /// Use default timeout, override for quick operation, and cancellation.
    /// </summary>
    public static async Task TimeoutAndCancellationExamples(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
        #region TimeoutandCancellationExamples
        // Use default timeout
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Override timeout for quick operation
        await driver.Session.StatusAsync(null, timeoutOverride: TimeSpan.FromSeconds(5));

        // With cancellation
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
        await driver.BrowsingContext.NavigateAsync(navParams, cancellationToken: cts.Token);
        #endregion
    }
}
