// <copyright file="RemoteBrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Browser locator settings for a remote browser, which is a browser that is not launched locally by WebDriverBiDi.NET,
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
    /// <param name="hostName">The hostname where the remote browser is running.</param>
    /// <param name="useSsl">A value indicating whether to use SSL for the connection.</param>
    public RemoteBrowserLocatorSettings(string browserName, string hostName, bool useSsl = false)
    {
        this.browserName = browserName;
        this.BrowserDisplayName = $"remote {browserName}";
        this.LocationBehavior = FileLocationBehavior.UseCustomLocation;
        this.ExpectedExecutablePath = $"{(useSsl ? "https" : "http")}://{hostName}";
    }

    /// <summary>
    /// Gets the name of the browser (e.g., "chrome", "firefox").
    /// </summary>
    public override string BrowserName => this.browserName;

    /// <summary>
    /// Gets the description of the browser location behavior, which is used for logging and user-facing messages.
    /// </summary>
    public override string BrowserLocationBehaviorDescription => this.BrowserDisplayName;

    /// <summary>
    /// Gets the name of the driver executable. Not implemented for this locator.
    /// </summary>
    public override string DriverExecutableName => throw new NotImplementedException();

    /// <summary>
    /// Gets the name of the environment variable that can be used to override the driver executable path. Not implemented for this locator.
    /// </summary>
    public override string DriverEnvironmentVariableName => throw new NotImplementedException();

    /// <summary>
    /// Gets the browser download information.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    /// <exception cref="NotImplementedException">Thrown because browser download is not supported for remote browsers.</exception>
    public override Task<BrowserDownloadInfo> GetBrowserDownloadInfo()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the driver download information for a driver that is compatible with this browser.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the driver download information as the result.</returns>
    /// <exception cref="NotImplementedException">Thrown because driver download is not supported for remote browsers.</exception>
    public override Task<DriverDownloadInfo> GetMatchingDriverDownloadInfo()
    {
        throw new NotImplementedException();
    }
}
