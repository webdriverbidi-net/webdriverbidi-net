// <copyright file="Installer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Tool;

using WebDriverBiDi.Browsers;

/// <summary>
/// Installs and resolves targets with the WebDriverBiDi.Browsers locators, and matches them against the cache.
/// </summary>
internal static class Installer
{
    /// <summary>
    /// Checks that a target can be installed.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <exception cref="ArgumentException">Thrown when the target's spec cannot be installed.</exception>
    public static void ValidateForInstall(Target target)
    {
        if (target.SpecKind == TargetSpecKind.Milestone && target.Name is not ("chrome" or "chrome-headless-shell" or "chromedriver"))
        {
            throw new ArgumentException($"{target.Text}: only Chrome and chromedriver are requested by milestone.");
        }

        if ((target.SpecKind is TargetSpecKind.Milestone or TargetSpecKind.Version) && target.Name == "msedgedriver")
        {
            throw new ArgumentException($"{target.Text}: msedgedriver is always the version of the installed Edge; name its channel, if any.");
        }

        if (target.SpecKind != TargetSpecKind.None && target.Name == "geckodriver")
        {
            throw new ArgumentException($"{target.Text}: geckodriver is always its latest release.");
        }
    }

    /// <summary>
    /// Checks that a target can select installations to remove from the cache.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <exception cref="ArgumentException">Thrown when the target names a channel of a driver.</exception>
    public static void ValidateForRemoval(Target target)
    {
        if (target.SpecKind == TargetSpecKind.Channel && target.IsDriver)
        {
            throw new ArgumentException($"{target.Text}: drivers are not cached by channel; name a version, or none.");
        }
    }

    /// <summary>
    /// Installs a target, if it is not already in the cache.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="options">The download options.</param>
    /// <param name="cancellationToken">A token that cancels the installation.</param>
    /// <returns>The path of the installed executable.</returns>
    public static async Task<string> InstallAsync(Target target, BrowserDownloadOptions options, CancellationToken cancellationToken)
    {
        if (target.IsDriver)
        {
            string? driverPath = await DriverLocator.FindDriverAsync(GetBrowser(target), target.Channel, GetVersion(target), downloadOptions: options, cancellationToken: cancellationToken).ConfigureAwait(false);
            return driverPath!;
        }

        return await BrowserLocator.FindBrowserAsync(GetBrowser(target), target.Channel, GetVersion(target), downloadOptions: options, browserOptions: GetBrowserOptions(target), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves the build a target installs, without downloading it.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="options">The download options.</param>
    /// <param name="cancellationToken">A token that cancels resolving the build.</param>
    /// <returns>The build.</returns>
    public static Task<ResolvedDownload> ResolveAsync(Target target, BrowserDownloadOptions options, CancellationToken cancellationToken)
    {
        return target.IsDriver
            ? DriverLocator.ResolveDownloadAsync(GetBrowser(target), target.Channel, GetVersion(target), downloadOptions: options, cancellationToken: cancellationToken)
            : BrowserLocator.ResolveDownloadAsync(GetBrowser(target), target.Channel, GetVersion(target), options, GetBrowserOptions(target), cancellationToken);
    }

    /// <summary>
    /// Gets a value indicating whether a target selects an installation in the cache.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="installation">The installation.</param>
    /// <returns><see langword="true"/> if the target selects the installation; otherwise, <see langword="false"/>.</returns>
    public static bool Selects(Target target, CachedInstallation installation)
    {
        return installation.Name == target.Name && target.SpecKind switch
        {
            TargetSpecKind.Channel => installation.Channel == target.Spec,
            TargetSpecKind.Milestone => installation.Version.StartsWith($"{target.Spec}.", StringComparison.Ordinal),
            TargetSpecKind.Version => installation.Version == target.Spec,
            _ => true,
        };
    }

    private static BrowserKind GetBrowser(Target target) => target.Name switch
    {
        "firefox" or "geckodriver" => BrowserKind.Firefox,
        "msedgedriver" => BrowserKind.Edge,
        _ => BrowserKind.Chrome,
    };

    private static BrowserVersion GetVersion(Target target) => target.SpecKind switch
    {
        TargetSpecKind.Milestone => BrowserVersion.Milestone(int.Parse(target.Spec, System.Globalization.CultureInfo.InvariantCulture)),
        TargetSpecKind.Version => BrowserVersion.Specific(target.Spec),
        _ => BrowserVersion.Latest,
    };

    private static BrowserLaunchOptions? GetBrowserOptions(Target target) => target.Name == "chrome-headless-shell" ? new ChromeLaunchOptions() { UseHeadlessShell = true } : null;
}
