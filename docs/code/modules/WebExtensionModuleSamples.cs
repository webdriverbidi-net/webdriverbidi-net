// <copyright file="WebExtensionModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/webextension.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.WebExtension;

/// <summary>
/// Snippets for WebExtension module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class WebExtensionModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
#region AccessingModule
        WebExtensionModule webExtension = driver.WebExtension;
#endregion
    }

    /// <summary>
    /// Install from path (packaged extension).
    /// </summary>
    public static async Task InstallFromPath(BiDiDriver driver)
    {
#region InstallfromPath
        InstallCommandParameters @params = new InstallCommandParameters(
            new ExtensionArchivePath("/path/to/extension.zip"));

        InstallCommandResult result = await driver.WebExtension.InstallAsync(@params);
        string extensionId = result.ExtensionId;

        Console.WriteLine($"Extension installed: {extensionId}");
#endregion
    }

    /// <summary>
    /// Install unpacked extension from directory.
    /// </summary>
    public static async Task InstallUnpackedExtension(BiDiDriver driver)
    {
#region InstallUnpackedExtension
        // Install from unpacked extension directory
        InstallCommandParameters @params = new InstallCommandParameters(
            new ExtensionPath("/path/to/extension-directory"));

        InstallCommandResult result = await driver.WebExtension.InstallAsync(@params);
        Console.WriteLine($"Extension installed: {result.ExtensionId}");
#endregion
    }

    /// <summary>
    /// Uninstall by ID.
    /// </summary>
    public static async Task UninstallById(BiDiDriver driver, string extensionId)
    {
#region UninstallbyID
        UninstallCommandParameters @params = new UninstallCommandParameters(extensionId);
        await driver.WebExtension.UninstallAsync(@params);

        Console.WriteLine("Extension uninstalled");
#endregion
    }

    /// <summary>
    /// Testing with extension.
    /// </summary>
    public static async Task TestingWithExtension(BiDiDriver driver, string contextId)
    {
#region TestingwithExtension
        // Install extension
        InstallCommandParameters installParams = new InstallCommandParameters(
            new ExtensionPath("/path/to/my-extension"));

        InstallCommandResult installResult =
            await driver.WebExtension.InstallAsync(installParams);
        string extensionId = installResult.ExtensionId;

        Console.WriteLine($"Extension installed: {extensionId}");

        // Navigate and test
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com")
            { Wait = ReadinessState.Complete });

        // Test extension functionality
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('[data-extension-injected]') !== null",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success &&
            success.Result is BooleanRemoteValue boolValue)
        {
            bool extensionActive = boolValue.Value;
            Console.WriteLine($"Extension active: {extensionActive}");
        }

        // Clean up
        await driver.WebExtension.UninstallAsync(
            new UninstallCommandParameters(extensionId));
#endregion
    }

    /// <summary>
    /// Testing extension content scripts.
    /// </summary>
    public static async Task TestingExtensionContentScripts(BiDiDriver driver, string contextId)
    {
#region TestingExtensionContentScripts
        // Install extension with content script
        InstallCommandParameters @params = new InstallCommandParameters(
            new ExtensionPath("/path/to/content-script-extension"));

        InstallCommandResult result = await driver.WebExtension.InstallAsync(@params);

        // Navigate to test page
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com")
            { Wait = ReadinessState.Complete });

        await Task.Delay(1000);

        // Check if content script modified the page
        EvaluateResult evalResult = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.body.dataset.contentScriptLoaded",
                new ContextTarget(contextId),
                true));

        if (evalResult is EvaluateResultSuccess success &&
            success.Result is StringRemoteValue loadedValue)
        {
            string loaded = loadedValue.Value;
            Console.WriteLine($"Content script loaded: {loaded}");
        }

        // Clean up
        await driver.WebExtension.UninstallAsync(
            new UninstallCommandParameters(result.ExtensionId));
#endregion
    }

    /// <summary>
    /// Testing multiple extensions.
    /// </summary>
    public static async Task TestingMultipleExtensions(BiDiDriver driver)
    {
#region TestingMultipleExtensions
        List<string> extensionIds = new List<string>();

        // Install multiple extensions
        string[] extensionPaths = new[]
        {
            "/path/to/extension1",
            "/path/to/extension2",
            "/path/to/extension3"
        };

        foreach (string path in extensionPaths)
        {
            InstallCommandParameters @params = new InstallCommandParameters(
                new ExtensionPath(path));

            InstallCommandResult result = await driver.WebExtension.InstallAsync(@params);
            extensionIds.Add(result.ExtensionId);

            Console.WriteLine($"Installed: {result.ExtensionId}");
        }

        // Run tests with all extensions active
        // ...

        // Clean up all extensions
        foreach (string id in extensionIds)
        {
            await driver.WebExtension.UninstallAsync(
                new UninstallCommandParameters(id));
        }
#endregion
    }

    /// <summary>
    /// Testing extension permissions.
    /// </summary>
    public static async Task TestingExtensionPermissions(BiDiDriver driver, string contextId)
    {
#region TestingExtensionPermissions
        // Install extension that requires permissions
        InstallCommandParameters @params = new InstallCommandParameters(
            new ExtensionPath("/path/to/permission-extension"));

        InstallCommandResult result = await driver.WebExtension.InstallAsync(@params);

        // Navigate to page where extension needs permissions
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com")
            { Wait = ReadinessState.Complete });

        // Test that extension has required permissions
        EvaluateResult evalResult = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                @"new Promise((resolve) => {
                    chrome.permissions.contains({
                        permissions: ['storage']
                    }, (result) => resolve(result));
                })",
                new ContextTarget(contextId),
                true));

        if (evalResult is EvaluateResultSuccess success &&
            success.Result is BooleanRemoteValue hasPermissionValue)
        {
            bool hasPermission = hasPermissionValue.Value;
            Console.WriteLine($"Extension has storage permission: {hasPermission}");
        }

        // Uninstall
        await driver.WebExtension.UninstallAsync(
            new UninstallCommandParameters(result.ExtensionId));
#endregion
    }
}
