// <copyright file="EmulationModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/emulation.md

#pragma warning disable CS8600, CS8602, CS0219

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Script;

/// <summary>
/// Snippets for Emulation module documentation. Compiled at build time to prevent API drift.
/// </summary>
public class EmulationModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
#region AccessingModule
        EmulationModule emulation = driver.Emulation;
#endregion
    }

    /// <summary>
    /// Set viewport size. Note: SetViewport is on BrowsingContext module.
    /// </summary>
    public static async Task SetViewportSize(BiDiDriver driver, string contextId)
    {
#region SetViewportSize
        SetViewportCommandParameters parameters = new SetViewportCommandParameters
        {
            BrowsingContextId = contextId,
            Viewport = new Viewport
            {
                Width = 375,    // iPhone width
                Height = 667    // iPhone height
            },
            DevicePixelRatio = 2.0
        };

        await driver.BrowsingContext.SetViewportAsync(parameters);
        Console.WriteLine("Viewport set to mobile size");
#endregion
    }

    /// <summary>
    /// Common device viewports.
    /// </summary>
    public static async Task CommonDeviceViewports(BiDiDriver driver, string contextId)
    {
#region CommonDeviceViewports
        // iPhone 12/13
        await driver.BrowsingContext.SetViewportAsync(new SetViewportCommandParameters
        {
            BrowsingContextId = contextId,
            Viewport = new Viewport { Width = 390, Height = 844 },
            DevicePixelRatio = 3.0
        });

        // iPad Pro
        await driver.BrowsingContext.SetViewportAsync(new SetViewportCommandParameters
        {
            BrowsingContextId = contextId,
            Viewport = new Viewport { Width = 1024, Height = 1366 },
            DevicePixelRatio = 2.0
        });

        // Desktop HD
        await driver.BrowsingContext.SetViewportAsync(new SetViewportCommandParameters
        {
            BrowsingContextId = contextId,
            Viewport = new Viewport { Width = 1920, Height = 1080 },
            DevicePixelRatio = 1.0
        });
#endregion
    }

    /// <summary>
    /// Set custom user agent.
    /// </summary>
    public static async Task SetCustomUserAgent(BiDiDriver driver, string contextId)
    {
#region SetCustomUserAgent
        SetUserAgentOverrideCommandParameters parameters = new SetUserAgentOverrideCommandParameters
        {
            UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1",
            Contexts = new List<string> { contextId }
        };

        await driver.Emulation.SetUserAgentOverrideAsync(parameters);
        Console.WriteLine("User agent set to iPhone");
#endregion
    }

    /// <summary>
    /// Common user agents.
    /// </summary>
    public static async Task CommonUserAgents(BiDiDriver driver, string contextId)
    {
#region CommonUserAgents
        // Mobile Safari (iPhone)
        string iPhoneUA = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1";

        // Mobile Chrome (Android)
        string androidUA = "Mozilla/5.0 (Linux; Android 12; Pixel 6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Mobile Safari/537.36";

        // Desktop Safari (macOS)
        string safariUA = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Safari/605.1.15";

        // Desktop Firefox
        string firefoxUA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:95.0) Gecko/20100101 Firefox/95.0";

        await driver.Emulation.SetUserAgentOverrideAsync(
            new SetUserAgentOverrideCommandParameters
            {
                UserAgent = iPhoneUA,
                Contexts = new List<string> { contextId }
            });
#endregion
    }

    /// <summary>
    /// Clear user agent override.
    /// </summary>
    public static async Task ClearUserAgentOverride(BiDiDriver driver, string contextId)
    {
#region ClearUserAgentOverride
        SetUserAgentOverrideCommandParameters parameters =
            SetUserAgentOverrideCommandParameters.ResetUserAgentOverride;
        parameters.Contexts = new List<string> { contextId };

        await driver.Emulation.SetUserAgentOverrideAsync(parameters);
        Console.WriteLine("User agent override cleared");
#endregion
    }

    /// <summary>
    /// Emulate dark mode using forced colors theme override.
    /// </summary>
    public static async Task EmulateDarkMode(BiDiDriver driver, string contextId)
    {
#region EmulateDarkMode
        SetForcedColorsModeThemeOverrideCommandParameters parameters =
            new SetForcedColorsModeThemeOverrideCommandParameters
            {
                Theme = ForcedColorsModeTheme.Dark,
                Contexts = new List<string> { contextId }
            };

        await driver.Emulation.SetForcedColorsModeThemeOverrideAsync(parameters);
        Console.WriteLine("Dark mode enabled");

        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "window.matchMedia('(prefers-color-scheme: dark)').matches",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success)
        {
            bool isDarkMode = success.Result.ConvertTo<BooleanRemoteValue>().Value;
            Console.WriteLine($"Dark mode active: {isDarkMode}");
        }
#endregion
    }

    /// <summary>
    /// Clear forced colors theme override.
    /// </summary>
    public static async Task ClearForcedColorsOverride(BiDiDriver driver, string contextId)
    {
#region ClearForcedColorsOverride
        // Use reset property of command parameters to clear the override
        SetForcedColorsModeThemeOverrideCommandParameters parameters =
            SetForcedColorsModeThemeOverrideCommandParameters.ResetForcedColorsModeThemeOverride;
        parameters.Contexts = new List<string> { contextId };

        await driver.Emulation.SetForcedColorsModeThemeOverrideAsync(parameters);
        Console.WriteLine("Forced colors override cleared");
#endregion
    }

    /// <summary>
    /// Set geolocation.
    /// </summary>
    public static async Task SetGeolocation(BiDiDriver driver, string contextId)
    {
#region SetGeolocation
        SetGeolocationOverrideCoordinatesCommandParameters parameters =
            new SetGeolocationOverrideCoordinatesCommandParameters
            {
                Coordinates = new GeolocationCoordinates(-122.4194, 37.7749) { Accuracy = 100 },
                Contexts = new List<string> { contextId }
            };

        await driver.Emulation.SetGeolocationOverrideAsync(parameters);
        Console.WriteLine("Geolocation set to San Francisco");

        // Verify location in page
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                @"new Promise((resolve) => {
                    navigator.geolocation.getCurrentPosition(
                        (pos) => resolve({
                            lat: pos.coords.latitude,
                            lng: pos.coords.longitude
                        })
                    );
                })",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success)
        {
            RemoteValueDictionary location = success.Result.ConvertTo<KeyValuePairCollectionRemoteValue>().Value;
            Console.WriteLine($"Browser location: {location["lat"].ConvertTo<DoubleRemoteValue>().Value}, {location["lng"].ConvertTo<DoubleRemoteValue>().Value}");
        }
#endregion
    }

    /// <summary>
    /// Common geolocation locations.
    /// </summary>
    public static async Task CommonLocations(BiDiDriver driver, string contextId)
    {
#region CommonLocations
        // New York
        await driver.Emulation.SetGeolocationOverrideAsync(
            new SetGeolocationOverrideCoordinatesCommandParameters
            {
                Coordinates = new GeolocationCoordinates(-74.0060, 40.7128) { Accuracy = 100 },
                Contexts = new List<string> { contextId }
            });

        // London
        await driver.Emulation.SetGeolocationOverrideAsync(
            new SetGeolocationOverrideCoordinatesCommandParameters
            {
                Coordinates = new GeolocationCoordinates(-0.1278, 51.5074) { Accuracy = 100 },
                Contexts = new List<string> { contextId }
            });

        // Tokyo
        await driver.Emulation.SetGeolocationOverrideAsync(
            new SetGeolocationOverrideCoordinatesCommandParameters
            {
                Coordinates = new GeolocationCoordinates(139.6503, 35.6762) { Accuracy = 100 },
                Contexts = new List<string> { contextId }
            });
#endregion
    }

    /// <summary>
    /// Clear geolocation override.
    /// </summary>
    public static async Task ClearGeolocationOverride(BiDiDriver driver, string contextId)
    {
#region ClearGeolocationOverride
        SetGeolocationOverrideCommandParameters parameters =
            SetGeolocationOverrideCommandParameters.ResetGeolocationOverrideCoordinates;
        parameters.Contexts = new List<string> { contextId };

        await driver.Emulation.SetGeolocationOverrideAsync(parameters);
        Console.WriteLine("Geolocation override cleared");
#endregion
    }

    /// <summary>
    /// Set timezone.
    /// </summary>
    public static async Task SetTimezone(BiDiDriver driver, string contextId)
    {
#region SetTimezone
        SetTimeZoneOverrideCommandParameters parameters = new SetTimeZoneOverrideCommandParameters
        {
            TimeZone = "America/Los_Angeles",
            Contexts = new List<string> { contextId }
        };

        await driver.Emulation.SetTimeZoneOverrideAsync(parameters);
        Console.WriteLine("Timezone set to Pacific Time");

        // Verify timezone
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "Intl.DateTimeFormat().resolvedOptions().timeZone",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success)
        {
            string timezone = success.Result.ConvertTo<StringRemoteValue>().Value;
            Console.WriteLine($"Browser timezone: {timezone}");
        }
#endregion
    }

    /// <summary>
    /// Common timezones.
    /// </summary>
    public static async Task CommonTimezones(BiDiDriver driver, string contextId)
    {
#region CommonTimezones
        // Pacific Time (US West Coast)
        await driver.Emulation.SetTimeZoneOverrideAsync(
            new SetTimeZoneOverrideCommandParameters
            {
                TimeZone = "America/Los_Angeles",
                Contexts = new List<string> { contextId }
            });

        // Eastern Time (US East Coast)
        await driver.Emulation.SetTimeZoneOverrideAsync(
            new SetTimeZoneOverrideCommandParameters
            {
                TimeZone = "America/New_York",
                Contexts = new List<string> { contextId }
            });

        // UTC
        await driver.Emulation.SetTimeZoneOverrideAsync(
            new SetTimeZoneOverrideCommandParameters
            {
                TimeZone = "UTC",
                Contexts = new List<string> { contextId }
            });

        // Tokyo
        await driver.Emulation.SetTimeZoneOverrideAsync(
            new SetTimeZoneOverrideCommandParameters
            {
                TimeZone = "Asia/Tokyo",
                Contexts = new List<string> { contextId }
            });
#endregion
    }

    /// <summary>
    /// Clear timezone override.
    /// </summary>
    public static async Task ClearTimezoneOverride(BiDiDriver driver, string contextId)
    {
#region ClearTimezoneOverride
        SetTimeZoneOverrideCommandParameters parameters =
            SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;
        parameters.Contexts = new List<string> { contextId };

        await driver.Emulation.SetTimeZoneOverrideAsync(parameters);
        Console.WriteLine("Timezone override cleared");
#endregion
    }

    /// <summary>
    /// Set locale.
    /// </summary>
    public static async Task SetLocale(BiDiDriver driver, string contextId)
    {
#region SetLocale
        SetLocaleOverrideCommandParameters parameters = new SetLocaleOverrideCommandParameters
        {
            Locale = "fr-FR",
            Contexts = new List<string> { contextId }
        };

        await driver.Emulation.SetLocaleOverrideAsync(parameters);
        Console.WriteLine("Locale set to French");

        // Verify locale
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "navigator.language",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success)
        {
            string locale = success.Result.ConvertTo<StringRemoteValue>().Value;
            Console.WriteLine($"Browser locale: {locale}");
        }
#endregion
    }

    /// <summary>
    /// Clear locale override.
    /// </summary>
    public static async Task ClearLocaleOverride(BiDiDriver driver, string contextId)
    {
#region ClearLocaleOverride
        SetLocaleOverrideCommandParameters parameters =
            SetLocaleOverrideCommandParameters.ResetLocaleOverride;
        parameters.Contexts = new List<string> { contextId };

        await driver.Emulation.SetLocaleOverrideAsync(parameters);
        Console.WriteLine("Locale override cleared");
#endregion
    }

    /// <summary>
    /// Emulate offline network conditions.
    /// </summary>
    public static async Task EmulateOffline(BiDiDriver driver, string contextId)
    {
#region EmulateOffline
        SetNetworkConditionsCommandParameters parameters = new SetNetworkConditionsCommandParameters
        {
            NetworkConditions = new NetworkConditionsOffline(),
            Contexts = new List<string> { contextId }
        };

        await driver.Emulation.SetNetworkConditionsAsync(parameters);
        Console.WriteLine("Network set to offline");
#endregion
    }

    /// <summary>
    /// Clear network conditions override.
    /// </summary>
    public static async Task ClearNetworkConditionsOverride(BiDiDriver driver, string contextId)
    {
#region ClearNetworkConditionsOverride
        SetNetworkConditionsCommandParameters parameters =
            SetNetworkConditionsCommandParameters.ResetNetworkConditions;
        parameters.Contexts = new List<string> { contextId };

        await driver.Emulation.SetNetworkConditionsAsync(parameters);
        Console.WriteLine("Network conditions override cleared");
#endregion
    }

    /// <summary>
    /// Set portrait screen orientation.
    /// </summary>
    public static async Task SetPortraitOrientation(BiDiDriver driver, string contextId)
    {
#region SetPortraitOrientation
        SetScreenOrientationOverrideCommandParameters parameters =
            new SetScreenOrientationOverrideCommandParameters
            {
                ScreenOrientation = new ScreenOrientation(
                    ScreenOrientationNatural.Portrait,
                    ScreenOrientationType.PortraitPrimary),
                Contexts = new List<string> { contextId }
            };

        await driver.Emulation.SetScreenOrientationOverrideAsync(parameters);
        Console.WriteLine("Screen orientation set to portrait");
#endregion
    }

    /// <summary>
    /// Set landscape screen orientation.
    /// </summary>
    public static async Task SetLandscapeOrientation(BiDiDriver driver, string contextId)
    {
#region SetLandscapeOrientation
        SetScreenOrientationOverrideCommandParameters parameters =
            new SetScreenOrientationOverrideCommandParameters
            {
                ScreenOrientation = new ScreenOrientation(
                    ScreenOrientationNatural.Landscape,
                    ScreenOrientationType.LandscapePrimary),
                Contexts = new List<string> { contextId }
            };

        await driver.Emulation.SetScreenOrientationOverrideAsync(parameters);
        Console.WriteLine("Screen orientation set to landscape");
#endregion
    }

    /// <summary>
    /// Clear screen orientation override.
    /// </summary>
    public static async Task ClearScreenOrientationOverride(BiDiDriver driver, string contextId)
    {
#region ClearScreenOrientationOverride
        SetScreenOrientationOverrideCommandParameters parameters =
            SetScreenOrientationOverrideCommandParameters.ResetScreenOrientationOverride;
        parameters.Contexts = new List<string> { contextId };

        await driver.Emulation.SetScreenOrientationOverrideAsync(parameters);
        Console.WriteLine("Screen orientation override cleared");
#endregion
    }

    /// <summary>
    /// Set screen dimensions.
    /// </summary>
    public static async Task SetScreenDimensions(BiDiDriver driver, string contextId)
    {
#region SetScreenDimensions
        SetScreenSettingsOverrideCommandParameters parameters =
            new SetScreenSettingsOverrideCommandParameters
            {
                ScreenArea = new ScreenArea { Width = 1920, Height = 1080 },
                Contexts = new List<string> { contextId }
            };

        await driver.Emulation.SetScreenSettingsOverrideAsync(parameters);
        Console.WriteLine("Screen dimensions set to 1920x1080");
#endregion
    }

    /// <summary>
    /// Clear screen settings override.
    /// </summary>
    public static async Task ClearScreenSettingsOverride(BiDiDriver driver, string contextId)
    {
#region ClearScreenSettingsOverride
        SetScreenSettingsOverrideCommandParameters parameters =
            SetScreenSettingsOverrideCommandParameters.ResetScreenSettingsOverride;
        parameters.Contexts = new List<string> { contextId };

        await driver.Emulation.SetScreenSettingsOverrideAsync(parameters);
        Console.WriteLine("Screen settings override cleared");
#endregion
    }

    /// <summary>
    /// Disable JavaScript for a context.
    /// </summary>
    public static async Task DisableJavaScript(BiDiDriver driver, string contextId)
    {
#region DisableJavaScript
        SetScriptingEnabledCommandParameters parameters = new SetScriptingEnabledCommandParameters
        {
            IsScriptingEnabled = false,
            Contexts = new List<string> { contextId }
        };

        await driver.Emulation.SetScriptingEnabledAsync(parameters);
        Console.WriteLine("JavaScript disabled");
#endregion
    }

    /// <summary>
    /// Clear scripting override.
    /// </summary>
    public static async Task ClearScriptingOverride(BiDiDriver driver, string contextId)
    {
#region ClearScriptingOverride
        SetScriptingEnabledCommandParameters parameters =
            SetScriptingEnabledCommandParameters.ResetScriptingEnabled;
        parameters.Contexts = new List<string> { contextId };

        await driver.Emulation.SetScriptingEnabledAsync(parameters);
        Console.WriteLine("Scripting override cleared");
#endregion
    }

    /// <summary>
    /// Set overlay scrollbars.
    /// </summary>
    public static async Task SetOverlayScrollbars(BiDiDriver driver, string contextId)
    {
#region SetOverlayScrollbars
        SetScrollbarTypeOverrideCommandParameters parameters =
            new SetScrollbarTypeOverrideCommandParameters
            {
                ScrollbarType = ScrollbarType.Overlay,
                Contexts = new List<string> { contextId }
            };

        await driver.Emulation.SetScrollbarTypeOverrideAsync(parameters);
        Console.WriteLine("Scrollbar type set to overlay");
#endregion
    }

    /// <summary>
    /// Clear scrollbar type override.
    /// </summary>
    public static async Task ClearScrollbarTypeOverride(BiDiDriver driver, string contextId)
    {
#region ClearScrollbarTypeOverride
        SetScrollbarTypeOverrideCommandParameters parameters =
            SetScrollbarTypeOverrideCommandParameters.ResetScrollbarTypeOverride;
        parameters.Contexts = new List<string> { contextId };

        await driver.Emulation.SetScrollbarTypeOverrideAsync(parameters);
        Console.WriteLine("Scrollbar type override cleared");
#endregion
    }

    /// <summary>
    /// Enable touch emulation with multiple touch points.
    /// </summary>
    public static async Task EnableTouchEmulation(BiDiDriver driver, string contextId)
    {
#region EnableTouchEmulation
        SetTouchOverrideCommandParameters parameters = new SetTouchOverrideCommandParameters
        {
            MaxTouchPoints = 5,
            Contexts = new List<string> { contextId }
        };

        await driver.Emulation.SetTouchOverrideAsync(parameters);
        Console.WriteLine("Touch emulation enabled with 5 touch points");
#endregion
    }

    /// <summary>
    /// Clear touch override.
    /// </summary>
    public static async Task ClearTouchOverride(BiDiDriver driver, string contextId)
    {
#region ClearTouchOverride
        SetTouchOverrideCommandParameters parameters =
            SetTouchOverrideCommandParameters.ResetTouchOverride;
        parameters.Contexts = new List<string> { contextId };

        await driver.Emulation.SetTouchOverrideAsync(parameters);
        Console.WriteLine("Touch override cleared");
#endregion
    }

    /// <summary>
    /// Pattern: Mobile device emulation.
    /// </summary>
#region MobileDeviceEmulation
    public async Task EmulateMobileDevice(
        BiDiDriver driver,
        string contextId,
        string deviceName)
    {
        switch (deviceName.ToLower())
        {
            case "iphone":
                await driver.BrowsingContext.SetViewportAsync(
                    new SetViewportCommandParameters
                    {
                        BrowsingContextId = contextId,
                        Viewport = new Viewport { Width = 390, Height = 844 },
                        DevicePixelRatio = 3.0
                    });

                await driver.Emulation.SetUserAgentOverrideAsync(
                    new SetUserAgentOverrideCommandParameters
                    {
                        UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1",
                        Contexts = new List<string> { contextId }
                    });
                break;

            case "android":
                await driver.BrowsingContext.SetViewportAsync(
                    new SetViewportCommandParameters
                    {
                        BrowsingContextId = contextId,
                        Viewport = new Viewport { Width = 412, Height = 915 },
                        DevicePixelRatio = 2.625
                    });

                await driver.Emulation.SetUserAgentOverrideAsync(
                    new SetUserAgentOverrideCommandParameters
                    {
                        UserAgent = "Mozilla/5.0 (Linux; Android 12; Pixel 6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Mobile Safari/537.36",
                        Contexts = new List<string> { contextId }
                    });
                break;

            case "tablet":
                await driver.BrowsingContext.SetViewportAsync(
                    new SetViewportCommandParameters
                    {
                        BrowsingContextId = contextId,
                        Viewport = new Viewport { Width = 1024, Height = 1366 },
                        DevicePixelRatio = 2.0
                    });
                break;
        }

        Console.WriteLine($"Emulating {deviceName}");
    }
#endregion

    public async Task EmulateMobileDeviceUsage(BiDiDriver driver, string contextId)
    {
#region MobileDeviceEmulationUsage
        // Usage
        await EmulateMobileDevice(driver, contextId, "iphone");
#endregion
    }

    /// <summary>
    /// Pattern: Responsive testing at multiple viewport sizes.
    /// </summary>
    public static async Task ResponsiveTesting(BiDiDriver driver, string contextId)
    {
#region ResponsiveTesting
        // Test at multiple viewport sizes
        List<(int Width, int Height, string Name)> viewports = new()
        {
            (320, 568, "Mobile Small"),
            (375, 667, "Mobile Medium"),
            (414, 896, "Mobile Large"),
            (768, 1024, "Tablet"),
            (1920, 1080, "Desktop")
        };

        foreach ((int Width, int Height, string Name) viewport in viewports)
        {
            Console.WriteLine($"\nTesting {viewport.Name} ({viewport.Width}x{viewport.Height})");

            await driver.BrowsingContext.SetViewportAsync(
                new SetViewportCommandParameters
                {
                    BrowsingContextId = contextId,
                    Viewport = new Viewport { Width = (ulong)viewport.Width, Height = (ulong)viewport.Height },
                    DevicePixelRatio = 1.0
                });

            // Take screenshot
            CaptureScreenshotCommandResult screenshot =
                await driver.BrowsingContext.CaptureScreenshotAsync(
                    new CaptureScreenshotCommandParameters(contextId));

            byte[] imageBytes = Convert.FromBase64String(screenshot.Data);
            await File.WriteAllBytesAsync(
                $"screenshot-{viewport.Name.Replace(" ", "-")}.png",
                imageBytes);
        }
#endregion
    }

    /// <summary>
    /// Pattern: Dark mode testing.
    /// </summary>
    public static async Task DarkModeTesting(BiDiDriver driver, string contextId)
    {
#region DarkModeTesting
        // Test both light and dark modes
        string[] colorSchemes = { "light", "dark" };

        foreach (string scheme in colorSchemes)
        {
            Console.WriteLine($"\nTesting {scheme} mode");

            ForcedColorsModeTheme theme = scheme == "dark" ? ForcedColorsModeTheme.Dark : ForcedColorsModeTheme.Light;
            await driver.Emulation.SetForcedColorsModeThemeOverrideAsync(
                new SetForcedColorsModeThemeOverrideCommandParameters
                {
                    Theme = theme,
                    Contexts = new List<string> { contextId }
                });

            // Navigate to page
            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com")
                { Wait = ReadinessState.Complete });

            // Verify color scheme applied
            EvaluateResult result = await driver.Script.EvaluateAsync(
                new EvaluateCommandParameters(
                    "getComputedStyle(document.body).backgroundColor",
                    new ContextTarget(contextId),
                    true));

            if (result is EvaluateResultSuccess success)
            {
                string bgColor = success.Result.ConvertTo<StringRemoteValue>().Value;
                Console.WriteLine($"Background color: {bgColor}");
            }

            // Take screenshot
            CaptureScreenshotCommandResult screenshot =
                await driver.BrowsingContext.CaptureScreenshotAsync(
                    new CaptureScreenshotCommandParameters(contextId));

            byte[] imageBytes = Convert.FromBase64String(screenshot.Data);
            await File.WriteAllBytesAsync($"screenshot-{scheme}.png", imageBytes);
        }
#endregion
    }

    /// <summary>
    /// Pattern: Location-based testing.
    /// </summary>
    public static async Task LocationBasedTesting(BiDiDriver driver, string contextId)
    {
#region Location-BasedTesting
        // Test application behavior in different locations
        Dictionary<string, (double Lat, double Lng)> locations = new()
        {
            { "New York", (40.7128, -74.0060) },
            { "London", (51.5074, -0.1278) },
            { "Tokyo", (35.6762, 139.6503) },
            { "Sydney", (-33.8688, 151.2093) }
        };

        foreach (KeyValuePair<string, (double Lat, double Lng)> location in locations)
        {
            Console.WriteLine($"\nTesting from {location.Key}");

            await driver.Emulation.SetGeolocationOverrideAsync(
                new SetGeolocationOverrideCoordinatesCommandParameters
                {
                    Coordinates = new GeolocationCoordinates(location.Value.Lng, location.Value.Lat) { Accuracy = 100 },
                    Contexts = new List<string> { contextId }
                });

            // Navigate to location-aware page
            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com/location")
                { Wait = ReadinessState.Complete });

            // Check location detection
            EvaluateResult result = await driver.Script.EvaluateAsync(
                new EvaluateCommandParameters(
                    "document.querySelector('#detected-location')?.textContent",
                    new ContextTarget(contextId),
                    true));

            if (result is EvaluateResultSuccess success)
            {
                string? detectedLocation = success.Result.ConvertTo<StringRemoteValue>().Value;
                Console.WriteLine($"Detected: {detectedLocation}");
            }
        }
#endregion
    }
}
