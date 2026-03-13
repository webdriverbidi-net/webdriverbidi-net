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
    /// Create a new session.
    /// </summary>
    public static async Task NewSession(BiDiDriver driver)
    {
#region NewSession
        NewCommandParameters parameters = new NewCommandParameters();

        NewCommandResult result = await driver.Session.NewSessionAsync(parameters);

        string sessionId = result.SessionId;
        Console.WriteLine($"Session ID: {result.SessionId}");
        Console.WriteLine($"Browser: {result.Capabilities.BrowserName} {result.Capabilities.BrowserVersion}");
#endregion
    }

    /// <summary>
    /// Create a new session with capability requests.
    /// </summary>
    public static async Task NewSessionWithCapabilities(BiDiDriver driver)
    {
#region NewSessionWithCapabilities
        CapabilityRequest capabilities = new CapabilityRequest
        {
            BrowserName = "chrome",
            AcceptInsecureCertificates = true,
        };

        NewCommandParameters parameters = new NewCommandParameters
        {
            Capabilities = new CapabilitiesRequest
            {
                AlwaysMatch = capabilities,
            },
        };

        NewCommandResult result = await driver.Session.NewSessionAsync(parameters);
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
