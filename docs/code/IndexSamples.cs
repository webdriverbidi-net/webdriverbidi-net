// <copyright file="IndexSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/index.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

/// <summary>
/// Snippets for documentation index quick example.
/// </summary>
public static class IndexSamples
{
    /// <summary>
    /// Quick example - connect, navigate, evaluate, disconnect.
    /// </summary>
    public static async Task QuickExample(string webSocketUrl)
    {
#region QuickExample
        BiDiDriver driver = new(TimeSpan.FromSeconds(10));
        await driver.StartAsync(webSocketUrl);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new());
        string contextId = tree.ContextTree[0].BrowsingContextId;

        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com")
            {
                Wait = ReadinessState.Complete
            });

        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters("document.title", new ContextTarget(contextId), true));

        await driver.StopAsync();
#endregion
    }
}
