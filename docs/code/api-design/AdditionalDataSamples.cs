// <copyright file="AdditionalDataSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/advanced/api-design.md

namespace WebDriverBiDi.Docs.Code.ApiDesign;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Input;
using WebDriverBiDi.Network;

/// <summary>
/// Snippets for API design AdditionalData documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class AdditionalDataSamples
{
    /// <summary>
    /// Inject protocol extension fields via AdditionalData.
    /// </summary>
    public static async Task ProtocolExtensionsViaAdditionalData(BiDiDriver driver, string contextId)
    {
        #region ProtocolExtensionsviaAdditionalData
        NavigateCommandParameters parameters = new NavigateCommandParameters(contextId, "https://example.com");

        // Add vendor-specific or pre-standard extension fields
        parameters.AdditionalData["customOption"] = "customValue";
        parameters.AdditionalData["experimentalFlag"] = true;

        await driver.BrowsingContext.NavigateAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Reading vendor extension data from network events and empty command results.
    /// </summary>
    public static async Task ReadingVendorExtensionData(BiDiDriver driver, string contextId)
    {
        #region ReadingVendorExtensionData
        // Chromium adds goog:-prefixed properties inside the request and response objects.
        using EventObserver<BeforeRequestSentEventArgs> observer = driver.Network.OnBeforeRequestSent.AddObserver(e =>
        {
            if (e.Request.AdditionalData.TryGetValue("goog:resourceType", out object? resourceType))
            {
                Console.WriteLine($"Resource type: {resourceType}");
            }
        });

        // Every result exposes the two positions separately: properties inside the result object
        // (AdditionalData) and properties on the response envelope (AdditionalResponseProperties).
        ReleaseActionsCommandResult result = await driver.Input.ReleaseActionsAsync(new ReleaseActionsCommandParameters(contextId));
        foreach (KeyValuePair<string, object?> extension in result.AdditionalData)
        {
            Console.WriteLine($"result.{extension.Key} = {extension.Value}");
        }

        if (result.AdditionalResponseProperties.TryGetValue("goog:channel", out object? channel))
        {
            Console.WriteLine($"Envelope channel: {channel}");
        }
        #endregion
    }

    /// <summary>
    /// Command-level reset: a static property returning a configured parameters instance.
    /// </summary>
    public static async Task CommandLevelReset(BiDiDriver driver)
    {
        #region CommandLevelReset
        // ResetTimeZoneOverride returns a SetTimeZoneOverrideCommandParameters instance
        await driver.Emulation.SetTimeZoneOverrideAsync(
            SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride);
        #endregion
    }

    /// <summary>
    /// Property-level sentinel: a static value assigned to one property of the parameters object.
    /// </summary>
    public static async Task PropertyLevelSentinel(BiDiDriver driver)
    {
        #region PropertyLevelSentinel
        // Reset viewport only — leave device pixel ratio unchanged
        await driver.BrowsingContext.SetViewportAsync(
            new SetViewportCommandParameters
            {
                Viewport = SetViewportCommandParameters.ResetToDefaultViewport,
            });

        // Reset device pixel ratio only — leave viewport dimensions unchanged
        await driver.BrowsingContext.SetViewportAsync(
            new SetViewportCommandParameters
            {
                DevicePixelRatio = SetViewportCommandParameters.ResetToDefaultDevicePixelRatio,
            });
        #endregion
    }
}
