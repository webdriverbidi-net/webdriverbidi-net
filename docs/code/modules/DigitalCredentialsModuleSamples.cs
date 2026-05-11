// <copyright file="DigitalCredentialsModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/digital-credentials.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.DigitalCredentials;

/// <summary>
/// Snippets for DigitalCredentials module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class DigitalCredentialsModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
        #region AccessingModule
        DigitalCredentialsModule digitalCredentials = driver.DigitalCredentials;
        #endregion
    }

    /// <summary>
    /// Simulate a wallet that declines the credential request.
    /// </summary>
    public static async Task DeclineCredentialRequest(BiDiDriver driver)
    {
        #region DeclineCredentialRequest
        SetVirtualWalletBehaviorCommandParameters @params = new SetVirtualWalletBehaviorCommandParameters(
            VirtualWalletAction.Decline);

        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(@params);
        Console.WriteLine("Virtual wallet will decline credential requests");
        #endregion
    }

    /// <summary>
    /// Simulate a wallet that returns a predefined credential response.
    /// </summary>
    public static async Task RespondWithCredential(BiDiDriver driver)
    {
        #region RespondWithCredential
        Dictionary<string, object?> credentialResponse = new Dictionary<string, object?>
        {
            ["token"] = "eyJhbGciOiJFUzI1NiJ9...",
            ["format"] = "jwt_vc"
        };

        SetVirtualWalletBehaviorCommandParameters @params = new SetVirtualWalletBehaviorCommandParameters(
            VirtualWalletAction.Respond)
        {
            Response = credentialResponse
        };

        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(@params);
        Console.WriteLine("Virtual wallet will return the predefined credential response");
        #endregion
    }

    /// <summary>
    /// Simulate a wallet that leaves the request pending (for timeout testing).
    /// </summary>
    public static async Task SimulateWalletWait(BiDiDriver driver)
    {
        #region SimulateWalletWait
        SetVirtualWalletBehaviorCommandParameters @params = new SetVirtualWalletBehaviorCommandParameters(
            VirtualWalletAction.Wait);

        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(@params);
        Console.WriteLine("Virtual wallet will leave the credential request pending");
        #endregion
    }

    /// <summary>
    /// Clear the active virtual wallet behavior.
    /// </summary>
    public static async Task ClearWalletBehavior(BiDiDriver driver)
    {
        #region ClearWalletBehavior
        SetVirtualWalletBehaviorCommandParameters @params = new SetVirtualWalletBehaviorCommandParameters(
            VirtualWalletAction.Clear);

        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(@params);
        Console.WriteLine("Virtual wallet behavior cleared");
        #endregion
    }

    /// <summary>
    /// Scope wallet behavior to a specific browsing context.
    /// </summary>
    public static async Task ScopeToContext(BiDiDriver driver, string contextId)
    {
        #region ScopeToContext
        SetVirtualWalletBehaviorCommandParameters @params = new SetVirtualWalletBehaviorCommandParameters(
            VirtualWalletAction.Decline)
        {
            Context = contextId
        };

        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(@params);
        Console.WriteLine($"Virtual wallet will decline requests in context {contextId}");
        #endregion
    }

    /// <summary>
    /// Scope wallet behavior to a specific credential protocol.
    /// </summary>
    public static async Task ScopeToProtocol(BiDiDriver driver)
    {
        #region ScopeToProtocol
        Dictionary<string, object?> credentialResponse = new Dictionary<string, object?>
        {
            ["token"] = "eyJhbGciOiJFUzI1NiJ9..."
        };

        SetVirtualWalletBehaviorCommandParameters @params = new SetVirtualWalletBehaviorCommandParameters(
            VirtualWalletAction.Respond)
        {
            Protocol = "openid4vp",
            Response = credentialResponse
        };

        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(@params);
        Console.WriteLine("Virtual wallet will respond to openid4vp credential requests");
        #endregion
    }

    /// <summary>
    /// Test that a page handles a declined credential request gracefully.
    /// </summary>
    public static async Task TestDeclinedCredential(BiDiDriver driver, string contextId)
    {
        #region TestDeclinedCredential
        // Configure wallet to decline
        SetVirtualWalletBehaviorCommandParameters @params = new SetVirtualWalletBehaviorCommandParameters(
            VirtualWalletAction.Decline);
        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(@params);

        // Navigate to a page that requests digital credentials
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com/verify-id")
            { Wait = ReadinessState.Complete });

        Console.WriteLine("Page loaded; wallet will reject any credential request");
        #endregion
    }

    /// <summary>
    /// Test a successful credential presentation flow.
    /// </summary>
    public static async Task TestSuccessfulCredentialFlow(BiDiDriver driver, string contextId)
    {
        #region TestSuccessfulCredentialFlow
        // Build a minimal mdoc response
        Dictionary<string, object?> mdocResponse = new Dictionary<string, object?>
        {
            ["version"] = "1.0",
            ["documents"] = new List<object?>
            {
                new Dictionary<string, object?>
                {
                    ["docType"] = "org.iso.18013.5.1.mDL",
                    ["issuerSigned"] = "..."
                }
            }
        };

        // Configure wallet to respond with the prepared credential
        SetVirtualWalletBehaviorCommandParameters @params = new SetVirtualWalletBehaviorCommandParameters(
            VirtualWalletAction.Respond)
        {
            Protocol = "preview",
            Context = contextId,
            Response = mdocResponse
        };
        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(@params);

        // Navigate and trigger the credential request
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com/verify-id")
            { Wait = ReadinessState.Complete });

        Console.WriteLine("Wallet is ready to present the mDL credential");
        #endregion
    }
}
