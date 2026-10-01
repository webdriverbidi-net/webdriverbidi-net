// <copyright file="RemoteBrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

/// <summary>
/// Browser locator settings for a remote browser, which is a browser that is not launched locally by this library,
/// but instead is expected to be running and accessible at a specified hostname and port. This can be used to connect
/// to a browser running in a remote environment, such as a Selenium Grid node or a cloud testing service.
/// </summary>
internal class RemoteBrowserLocatorSettings : BrowserLocatorSettings
{
    private readonly string browserName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="browserName">The name of the browser used in the WebDriver Classic session creation capabilities object (e.g., "chrome", "firefox").</param>
    /// <param name="endpoint">The URL of the grid or browser endpoint.</param>
    public RemoteBrowserLocatorSettings(string browserName, Uri endpoint)
        : base(new BrowserDownloadOptions())
    {
        this.browserName = browserName;
        this.BrowserDisplayName = $"remote {browserName}";
        this.LocationBehavior = FileLocationBehavior.UseCustomLocation;
        this.ExpectedExecutablePath = new UriBuilder(endpoint) { UserName = string.Empty, Password = string.Empty }.Uri.AbsoluteUri;
    }

    /// <summary>
    /// Gets the name of the browser (e.g., "chrome", "firefox").
    /// </summary>
    public override string BrowserName => this.browserName;

    /// <summary>
    /// Gets the description of the browser location behavior, which is used for logging and user-facing messages.
    /// </summary>
    public override string BrowserLocationBehaviorDescription => this.BrowserDisplayName;
}
