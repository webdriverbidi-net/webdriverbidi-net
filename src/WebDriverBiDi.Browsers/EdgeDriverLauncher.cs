// <copyright file="EdgeDriverLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Object for launching Microsoft Edge to connect to using a WebDriverBiDi session using a local instance of the
/// msedgedriver browser driver executable.
/// </summary>
public class EdgeDriverLauncher : ChromeDriverLauncher
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EdgeDriverLauncher" /> class using Edge browser locator settings.
    /// The settings must have <see cref="BrowserLocatorSettings.IncludeDriver"/> set to true.
    /// </summary>
    /// <param name="settings">The Edge browser locator settings to use for locating the browser and driver executables.</param>
    internal EdgeDriverLauncher(EdgeBrowserLocatorSettings settings)
        : base(settings)
    {
    }

    /// <inheritdoc/>
    private protected override string BrowserNameCapabilityValue => "MicrosoftEdge";

    /// <inheritdoc/>
    private protected override string OptionsCapabilityName => "ms:edgeOptions";
}
