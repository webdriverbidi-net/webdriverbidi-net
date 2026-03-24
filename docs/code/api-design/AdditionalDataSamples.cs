// <copyright file="AdditionalDataSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/advanced/api-design.md

namespace WebDriverBiDi.Docs.Code.ApiDesign;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

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
}
