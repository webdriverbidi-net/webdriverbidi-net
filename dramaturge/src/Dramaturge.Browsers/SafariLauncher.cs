// <copyright file="SafariLauncher.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

/// <summary>
/// Object for launching a Safari browser to connect to using a WebDriverBiDi session
/// using a local instance of the safaridriver browser driver executable.
/// </summary>
public class SafariLauncher : ClassicDriverExecutableBrowserLauncher
{
    /// <summary>
    /// The name of the capability Safari requires, in addition to <c>webSocketUrl</c>, to enable WebDriver BiDi.
    /// </summary>
    internal const string ExperimentalWebSocketUrlCapabilityName = "safari:experimentalWebSocketUrl";

    private readonly bool isTechnologyPreview;

    /// <summary>
    /// Initializes a new instance of the <see cref="SafariLauncher" /> class using Safari browser locator settings.
    /// The settings must have <see cref="BrowserLocatorSettings.IncludeDriver"/> set to true.
    /// </summary>
    /// <param name="settings">The Safari browser locator settings to use for locating the browser and driver executables.</param>
    internal SafariLauncher(SafariBrowserLocatorSettings settings)
        : base(settings, 0)
    {
        this.isTechnologyPreview = settings.IsTechnologyPreview;
    }

    /// <summary>
    /// Gets a value indicating whether the browser can be closed using WebDriver BiDi's browser.close command.
    /// </summary>
    public override bool IsBrowserCloseAllowed => false;

    /// <inheritdoc/>
    internal override IReadOnlyCollection<string> LaunchCapabilityNames { get; } = ["browserName", "webSocketUrl", ExperimentalWebSocketUrlCapabilityName, "safari:options"];

    /// <summary>
    /// Creates the WebDriver Classic capabilities used to launch the browser.
    /// </summary>
    /// <returns>A dictionary containing the capabilities.</returns>
    protected override Dictionary<string, object?> CreateBrowserLaunchCapabilities()
    {
        // CONSIDER: This is a very naive and simple set of capabilities.
        // A future implementation could create a more fully-featured
        // generation of capabilities.
        // Note carefully the addition of the "safari:experimentalWebSocketUrl"
        // capability. This is mandatory to enable WebDriverBiDi for now, but
        // should not be necessary in the future.
        Dictionary<string, object?> capabilities = new()
        {
            ["browserName"] = "safari",
            ["webSocketUrl"] = true,
            [ExperimentalWebSocketUrlCapabilityName] = true,
        };

        if (this.isTechnologyPreview)
        {
            Dictionary<string, object> options = new()
            {
                ["technologyPreview"] = true,
            };

            capabilities["safari:options"] = options;
        }

        return capabilities;
    }
}
