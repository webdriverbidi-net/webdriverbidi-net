// <copyright file="AdditionalModulesSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/additional-modules.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.Bluetooth;
using WebDriverBiDi.Permissions;
using WebDriverBiDi.Session;
using WebDriverBiDi.Speculation;
using WebDriverBiDi.UserAgentClientHints;
using WebDriverBiDi.WebExtension;

/// <summary>
/// Snippets for additional modules documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class AdditionalModulesSamples
{
    /// <summary>
    /// Grant geolocation permission.
    /// </summary>
    public static async Task GrantGeolocationPermission(BiDiDriver driver)
    {
#region GrantGeolocationPermission
        // Grant geolocation permission
        SetPermissionCommandParameters parameters =
            new SetPermissionCommandParameters(
                new PermissionDescriptor("geolocation"),
                PermissionState.Granted,
                "https://example.org");

        await driver.Permissions.SetPermissionAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Simulate a Bluetooth peripheral (preconnected).
    /// </summary>
    public static async Task SimulateBluetoothDevice(BiDiDriver driver, string contextId)
    {
#region SimulateBluetoothDevice
        // Simulate a Bluetooth device
        SimulatePreconnectedPeripheralCommandParameters parameters =
            new SimulatePreconnectedPeripheralCommandParameters(
                contextId,
                "AA:BB:CC:DD:EE:FF",
                "Heart Rate Monitor"
            );

        await driver.Bluetooth.SimulatePreconnectedPeripheralAsync(parameters);
#endregion
    }

    /// <summary>
    /// Install a browser extension.
    /// </summary>
    public static async Task InstallExtension(BiDiDriver driver)
    {
#region InstallExtension
        // Install an extension
        InstallCommandParameters parameters = new InstallCommandParameters(
            new ExtensionArchivePath("/path/to/extension.crx")
        );

        InstallCommandResult result = await driver.WebExtension.InstallAsync(parameters);
        string extensionId = result.ExtensionId;

        Console.WriteLine($"Extension installed: {extensionId}");
#endregion
    }

    /// <summary>
    /// Override client hints for cross-browser brand testing.
    /// </summary>
    public static async Task SetClientHintsOverride(BiDiDriver driver)
    {
#region SetClientHintsOverride
        // Override client hints for cross-browser brand testing
        SetClientHintsOverrideCommandParameters parameters = new SetClientHintsOverrideCommandParameters();
        parameters.ClientHints = new ClientHintsMetadata
        {
            Brands = new List<BrandVersion>
            {
                new BrandVersion("Chromium", "120.0"),
                new BrandVersion("Google Chrome", "120.0")
            },
            Platform = "Windows",
            PlatformVersion = "10.0",
            Architecture = "x86",
            Mobile = false
        };

        await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);
#endregion
    }

    /// <summary>
    /// Subscribe to prefetch status updates from the Speculation module.
    /// </summary>
    public static async Task SubscribeToPrefetchStatus(BiDiDriver driver)
    {
#region SubscribetoPrefetchStatus
        // Subscribe to prefetch status updates
        driver.Speculation.OnPrefetchStatusUpdated.AddObserver((e) =>
        {
            Console.WriteLine($"Prefetch: {e.Url} -> {e.Status}");
        });

        SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
            driver.Speculation.OnPrefetchStatusUpdated.EventName);
        await driver.Session.SubscribeAsync(subscribe);
#endregion
    }
}
