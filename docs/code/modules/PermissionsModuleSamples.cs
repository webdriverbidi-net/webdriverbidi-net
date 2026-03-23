// <copyright file="PermissionsModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/permissions.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Permissions;
using WebDriverBiDi.Script;

/// <summary>
/// Snippets for Permissions module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class PermissionsModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
#region AccessingModule
        PermissionsModule permissions = driver.Permissions;
#endregion
    }

    /// <summary>
    /// Grant geolocation permission.
    /// </summary>
    public static async Task GrantGeolocationPermission(BiDiDriver driver)
    {
#region GrantGeolocationPermission
        SetPermissionCommandParameters @params = new SetPermissionCommandParameters(
            new PermissionDescriptor("geolocation"),
            PermissionState.Granted,
            "https://example.com");

        await driver.Permissions.SetPermissionAsync(@params);
        Console.WriteLine("Geolocation permission granted");
#endregion
    }

    /// <summary>
    /// Deny permission.
    /// </summary>
    public static async Task DenyPermission(BiDiDriver driver)
    {
#region DenyPermission
        SetPermissionCommandParameters @params = new SetPermissionCommandParameters(
            new PermissionDescriptor("geolocation"),
            PermissionState.Denied,
            "https://example.com");

        await driver.Permissions.SetPermissionAsync(@params);
        Console.WriteLine("Geolocation permission denied");
#endregion
    }

    /// <summary>
    /// Grant notifications permission.
    /// </summary>
    public static async Task GrantNotificationsPermission(BiDiDriver driver)
    {
#region GrantNotificationsPermission
        SetPermissionCommandParameters @params = new SetPermissionCommandParameters(
            new PermissionDescriptor("notifications"),
            PermissionState.Granted,
            "https://example.com");

        await driver.Permissions.SetPermissionAsync(@params);
#endregion
    }

    /// <summary>
    /// Grant camera permission.
    /// </summary>
    public static async Task GrantCameraPermission(BiDiDriver driver)
    {
#region GrantCameraPermission
        SetPermissionCommandParameters @params = new SetPermissionCommandParameters(
            new PermissionDescriptor("camera"),
            PermissionState.Granted,
            "https://example.com");

        await driver.Permissions.SetPermissionAsync(@params);
#endregion
    }

    /// <summary>
    /// Grant microphone permission.
    /// </summary>
    public static async Task GrantMicrophonePermission(BiDiDriver driver)
    {
#region GrantMicrophonePermission
        SetPermissionCommandParameters @params = new SetPermissionCommandParameters(
            new PermissionDescriptor("microphone"),
            PermissionState.Granted,
            "https://example.com");

        await driver.Permissions.SetPermissionAsync(@params);
#endregion
    }

    /// <summary>
    /// Grant MIDI permission.
    /// </summary>
    public static async Task GrantMidiPermission(BiDiDriver driver)
    {
#region GrantMIDIPermission
        SetPermissionCommandParameters @params = new SetPermissionCommandParameters(
            new PermissionDescriptor("midi"),
            PermissionState.Granted,
            "https://example.com");

        await driver.Permissions.SetPermissionAsync(@params);
#endregion
    }

    /// <summary>
    /// Set permission to prompt user.
    /// </summary>
    public static async Task SetPermissionToPrompt(BiDiDriver driver)
    {
#region SetPermissiontoPrompt
        // Set permission to prompt user
        SetPermissionCommandParameters @params = new SetPermissionCommandParameters(
            new PermissionDescriptor("notifications"),
            PermissionState.Prompt,
            "https://example.com");

        await driver.Permissions.SetPermissionAsync(@params);
#endregion
    }

    /// <summary>
    /// Testing geolocation functionality.
    /// </summary>
    public static async Task TestingGeolocationFunctionality(BiDiDriver driver, string contextId)
    {
#region TestingGeolocationFunctionality
        // Grant geolocation permission
        SetPermissionCommandParameters permParams = new SetPermissionCommandParameters(
            new PermissionDescriptor("geolocation"),
            PermissionState.Granted,
            "https://example.com");
        await driver.Permissions.SetPermissionAsync(permParams);

        // Set location
        SetGeolocationOverrideCoordinatesCommandParameters geoParams =
            new SetGeolocationOverrideCoordinatesCommandParameters
            {
                Coordinates = new GeolocationCoordinates(-122.4194, 37.7749) { Accuracy = 100 },
                Contexts = new List<string> { contextId }
            };
        await driver.Emulation.SetGeolocationOverrideAsync(geoParams);

        // Test location access
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                @"new Promise((resolve) => {
                    navigator.geolocation.getCurrentPosition(
                        (pos) => resolve(`${pos.coords.latitude},${pos.coords.longitude}`),
                        (err) => resolve(`Error: ${err.message}`)
                    );
                })",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success &&
            success.Result is StringRemoteValue locationValue)
        {
            string location = locationValue.Value;
            Console.WriteLine($"Location: {location}");
        }
#endregion
    }

    /// <summary>
    /// Testing notification permissions.
    /// </summary>
    public static async Task TestingNotificationPermissions(BiDiDriver driver, string contextId)
    {
#region TestingNotificationPermissions
        // Grant notification permission
        SetPermissionCommandParameters @params = new SetPermissionCommandParameters(
            new PermissionDescriptor("notifications"),
            PermissionState.Granted,
            "https://example.com");
        await driver.Permissions.SetPermissionAsync(@params);

        // Navigate to page
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com")
            { Wait = ReadinessState.Complete });

        // Check permission state in page
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "Notification.permission",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success &&
            success.Result is StringRemoteValue permissionValue)
        {
            string permissionState = permissionValue.Value;
            Console.WriteLine($"Notification permission: {permissionState}");
        }
#endregion
    }

    /// <summary>
    /// Testing permission denial.
    /// </summary>
    public static async Task TestingPermissionDenial(BiDiDriver driver, string contextId)
    {
#region TestingPermissionDenial
        // Deny camera permission
        SetPermissionCommandParameters @params = new SetPermissionCommandParameters(
            new PermissionDescriptor("camera"),
            PermissionState.Denied,
            "https://example.com");
        await driver.Permissions.SetPermissionAsync(@params);

        // Try to access camera - should fail
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                @"navigator.mediaDevices.getUserMedia({ video: true })
                    .then(() => 'granted')
                    .catch(err => `denied: ${err.name}`)",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success &&
            success.Result is StringRemoteValue accessValue)
        {
            string accessResult = accessValue.Value;
            Console.WriteLine($"Camera access: {accessResult}");
        }
#endregion
    }

    /// <summary>
    /// Testing multiple permissions.
    /// </summary>
    public static async Task TestingMultiplePermissions(BiDiDriver driver, string contextId)
    {
#region TestingMultiplePermissions
        // Grant multiple permissions
        string[] permissionTypes = { "camera", "microphone" };

        foreach (string permType in permissionTypes)
        {
            SetPermissionCommandParameters @params = new SetPermissionCommandParameters(
                new PermissionDescriptor(permType),
                PermissionState.Granted,
                "https://example.com");
            await driver.Permissions.SetPermissionAsync(@params);
        }

        // Test accessing both camera and microphone
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                @"navigator.mediaDevices.getUserMedia({ video: true, audio: true })
                    .then(() => 'both granted')
                    .catch(err => `error: ${err.name}`)",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success &&
            success.Result is StringRemoteValue accessValue)
        {
            string accessResult = accessValue.Value;
            Console.WriteLine($"Media access: {accessResult}");
        }
    }
#endregion
}
