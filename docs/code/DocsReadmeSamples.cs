// <copyright file="DocsReadmeSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/README.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code;

using WebDriverBiDi;

/// <summary>
/// Snippets for documentation README code examples.
/// </summary>
public static class DocsReadmeSamples
{
    /// <summary>
    /// Complete runnable example - create driver, connect, use, disconnect.
    /// </summary>
    public static async Task CompleteRunnableExample(string webSocketUrl)
    {
        #region CompleteRunnableExample
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
        await driver.StartAsync(webSocketUrl);

        // ... your code here ...

        await driver.StopAsync();
        #endregion
    }
}
