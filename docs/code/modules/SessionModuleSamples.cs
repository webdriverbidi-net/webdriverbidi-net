// <copyright file="SessionModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/session.md

namespace WebDriverBiDi.Docs.Code.Modules;

using WebDriverBiDi;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for Session module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class SessionModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
#region AccessingModule
        SessionModule session = driver.Session;
#endregion
    }

    /// <summary>
    /// Check session status.
    /// </summary>
    public static async Task CheckSessionStatus(BiDiDriver driver)
    {
#region CheckSessionStatus
        StatusCommandParameters parameters = new StatusCommandParameters();
        StatusCommandResult result = await driver.Session.StatusAsync(parameters);

        Console.WriteLine($"Is ready: {result.IsReady}");
        Console.WriteLine($"Message: {result.Message}");
#endregion
    }
}
