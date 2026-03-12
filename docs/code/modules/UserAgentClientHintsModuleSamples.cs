// <copyright file="UserAgentClientHintsModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/user-agent-client-hints.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.UserAgentClientHints;

/// <summary>
/// Snippets for UserAgentClientHints module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class UserAgentClientHintsModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
#region AccessingModule
        UserAgentClientHintsModule userAgentClientHints = driver.UserAgentClientHints;
#endregion
    }

    /// <summary>
    /// Basic client hints override.
    /// </summary>
    public static async Task BasicOverride(BiDiDriver driver)
    {
#region BasicOverride
        SetClientHintsOverrideCommandParameters parameters =
            new SetClientHintsOverrideCommandParameters();
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
        Console.WriteLine("Client hints override set");
#endregion
    }

    /// <summary>
    /// Common browser brands (Chrome, Firefox, Mobile Chrome).
    /// </summary>
    public static async Task CommonBrowserBrands(BiDiDriver driver)
    {
#region CommonBrowserBrands
        // Chrome on Windows
        SetClientHintsOverrideCommandParameters parameters =
            new SetClientHintsOverrideCommandParameters();
        parameters.ClientHints = new ClientHintsMetadata
        {
            Brands = new List<BrandVersion>
            {
                new BrandVersion("Chromium", "120.0"),
                new BrandVersion("Google Chrome", "120.0")
            },
            FullVersionList = new List<BrandVersion>
            {
                new BrandVersion("Chromium", "120.0.6099.109"),
                new BrandVersion("Google Chrome", "120.0.6099.109")
            },
            Platform = "Windows",
            PlatformVersion = "10.0",
            Architecture = "x86",
            Model = "",
            Mobile = false,
            Bitness = "64"
        };

        // Firefox on macOS
        parameters.ClientHints = new ClientHintsMetadata
        {
            Brands = new List<BrandVersion>
            {
                new BrandVersion("Not_A Brand", "8"),
                new BrandVersion("Firefox", "121.0")
            },
            Platform = "macOS",
            PlatformVersion = "14.0",
            Architecture = "arm",
            Mobile = false
        };

        // Mobile Chrome (Android)
        parameters.ClientHints = new ClientHintsMetadata
        {
            Brands = new List<BrandVersion>
            {
                new BrandVersion("Chromium", "120.0"),
                new BrandVersion("Google Chrome", "120.0")
            },
            Platform = "Android",
            PlatformVersion = "14.0",
            Architecture = "arm",
            Model = "Pixel 7",
            Mobile = true
        };

        await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);
#endregion
    }

    /// <summary>
    /// Reset client hints override.
    /// </summary>
    public static async Task ResetClientHints(BiDiDriver driver)
    {
#region ResetClientHints
        await driver.UserAgentClientHints.SetClientHintsOverrideAsync(
            SetClientHintsOverrideCommandParameters.ResetClientHintsOverride);
        Console.WriteLine("Client hints override cleared");
#endregion
    }

    /// <summary>
    /// Target specific browsing contexts.
    /// </summary>
    public static async Task TargetSpecificContexts(BiDiDriver driver, string contextId)
    {
#region TargetSpecificContexts
        SetClientHintsOverrideCommandParameters parameters =
            new SetClientHintsOverrideCommandParameters();
        parameters.ClientHints = new ClientHintsMetadata
        {
            Brands = new List<BrandVersion> { new BrandVersion("Chromium", "120.0") },
            Mobile = true
        };
        parameters.Contexts = new List<string> { contextId };

        await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);
#endregion
    }

    /// <summary>
    /// Target specific user contexts.
    /// </summary>
    public static async Task TargetSpecificUserContexts(BiDiDriver driver, string userContextId)
    {
#region TargetSpecificUserContexts
        SetClientHintsOverrideCommandParameters parameters =
            new SetClientHintsOverrideCommandParameters();
        parameters.ClientHints = new ClientHintsMetadata
        {
            Platform = "Linux",
            Architecture = "x86"
        };
        parameters.UserContexts = new List<string> { userContextId };

        await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);
#endregion
    }

    /// <summary>
    /// Mobile device testing pattern.
    /// </summary>
    public static async Task MobileDeviceTesting(BiDiDriver driver, string contextId)
    {
#region MobileDeviceTesting
        // Emulate mobile client hints for responsive design testing
        SetClientHintsOverrideCommandParameters parameters = new SetClientHintsOverrideCommandParameters();
        parameters.ClientHints = new ClientHintsMetadata
        {
            Brands = new List<BrandVersion>
            {
                new BrandVersion("Chromium", "120.0"),
                new BrandVersion("Google Chrome", "120.0")
            },
            Platform = "Android",
            PlatformVersion = "14.0",
            Architecture = "arm",
            Model = "Pixel 7",
            Mobile = true,
            FormFactors = new List<string> { "Mobile" }
        };

        await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);

        // Combine with Emulation module for full mobile emulation
        await driver.BrowsingContext.SetViewportAsync(
            new SetViewportCommandParameters
            {
                BrowsingContextId = contextId,
                Viewport = new Viewport { Width = 412, Height = 915 },
                DevicePixelRatio = 2.625
            });
#endregion
    }

    /// <summary>
    /// Cross-browser brand testing pattern.
    /// </summary>
    public static async Task CrossBrowserBrandTesting(BiDiDriver driver, string contextId)
    {
#region Cross-BrowserBrandTesting
        // Test how a site behaves with different browser brands
        Dictionary<string, ClientHintsMetadata> browserConfigs = new()
        {
            ["Chrome"] = new ClientHintsMetadata
            {
                Brands = new List<BrandVersion>
                {
                    new BrandVersion("Chromium", "120.0"),
                    new BrandVersion("Google Chrome", "120.0")
                },
                Platform = "Windows",
                Mobile = false
            },
            ["Edge"] = new ClientHintsMetadata
            {
                Brands = new List<BrandVersion>
                {
                    new BrandVersion("Chromium", "120.0"),
                    new BrandVersion("Microsoft Edge", "120.0")
                },
                Platform = "Windows",
                Mobile = false
            },
            ["Safari"] = new ClientHintsMetadata
            {
                Brands = new List<BrandVersion>
                {
                    new BrandVersion("Safari", "17.0")
                },
                Platform = "macOS",
                Mobile = false
            }
        };

        foreach (KeyValuePair<string, ClientHintsMetadata> config in browserConfigs)
        {
            Console.WriteLine($"\nTesting as {config.Key}");
            SetClientHintsOverrideCommandParameters parameters =
                new SetClientHintsOverrideCommandParameters();
            parameters.ClientHints = config.Value;
            await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);

            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com")
                { Wait = ReadinessState.Complete });

            // Verify site behavior for this browser brand
            EvaluateResult result = await driver.Script.EvaluateAsync(
                new EvaluateCommandParameters(
                    "navigator.userAgentData?.brands?.map(b => b.brand).join(', ') ?? 'not supported'",
                    new ContextTarget(contextId),
                    true));

            if (result is EvaluateResultSuccess success)
            {
                string brands = success.Result.ValueAs<string>();
                Console.WriteLine($"Detected brands: {brands}");
            }
        }
#endregion
    }

    /// <summary>
    /// Verify client hints in page.
    /// </summary>
    public static async Task VerifyClientHintsInPage(BiDiDriver driver, string contextId)
    {
#region VerifyClientHintsinPage
        // Set override
        SetClientHintsOverrideCommandParameters parameters =
            new SetClientHintsOverrideCommandParameters();
        parameters.ClientHints = new ClientHintsMetadata
        {
            Brands = new List<BrandVersion> { new BrandVersion("TestBrowser", "1.0") },
            Platform = "TestOS",
            Mobile = true
        };
        await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);

        // Verify via User-Agent Client Hints API (JavaScript)
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                @"(async () => {
                    const ua = navigator.userAgentData;
                    if (!ua) return 'User-Agent Client Hints API not supported';
                    const hints = await ua.getHighEntropyValues(['brands', 'platform', 'mobile']);
                    return JSON.stringify(hints);
                })()",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success)
        {
            string hintsJson = success.Result.ValueAs<string>();
            Console.WriteLine($"Client hints: {hintsJson}");
        }
#endregion
    }
}
