// <copyright file="EdgeBrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

using System.Runtime.InteropServices;

/// <summary>
/// Defines the locator settings for Microsoft Edge. Edge is never downloaded, because Microsoft publishes it only
/// as installers: the installed Edge of a channel, or the executable at a custom location, is used. Its driver,
/// msedgedriver, is downloaded in the version of the installed Edge.
/// </summary>
internal class EdgeBrowserLocatorSettings : BrowserLocatorSettings
{
    private const string DriverNameValue = "msedgedriver";

    private readonly EdgeChannel channelValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="EdgeBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="channel">The distribution channel of Edge.</param>
    /// <param name="downloadOptions">The options controlling where msedgedriver is cached and downloaded from.</param>
    /// <param name="customPath">The path to the Edge executable, or <see langword="null"/> for the installed Edge of the channel.</param>
    public EdgeBrowserLocatorSettings(EdgeChannel channel, BrowserDownloadOptions downloadOptions, string? customPath = null)
        : base(downloadOptions)
    {
        this.channelValue = channel;
        this.Channel = channel.ToString().ToLowerInvariant();
        this.BrowserDisplayName = channel == EdgeChannel.Stable ? "Microsoft Edge" : $"Microsoft Edge {channel}";
        this.EnvironmentVariableName = "EDGE_EXECUTABLE";
        this.LocationBehavior = customPath is null ? FileLocationBehavior.UseSystemInstallLocation : FileLocationBehavior.UseCustomLocation;
        this.InstallerFileName = string.Empty;
        this.ExpectedExecutablePath = customPath ?? this.GetDefaultSystemInstalledLocation();
        this.Version = SystemVersionString;
    }

    /// <summary>
    /// Gets the name of the browser.
    /// </summary>
    public override string BrowserName { get; } = "edge";

    /// <summary>
    /// Gets the name of the driver executable ("msedgedriver" or "msedgedriver.exe").
    /// </summary>
    public override string DriverExecutableName => this.Platform.OperatingSystem == OperatingSystemFamily.Windows ? $"{DriverNameValue}.exe" : DriverNameValue;

    /// <summary>
    /// Gets the name of the environment variable that can be used to override the driver executable path.
    /// </summary>
    public override string DriverEnvironmentVariableName => "MSEDGEDRIVER_EXECUTABLE";

    private BrowserPlatform Platform => this.DownloadOptions.ResolvedPlatform;

    /// <summary>
    /// Reads the version of the Edge at a path, which the driver must match.
    /// </summary>
    /// <param name="executablePath">The path of the Edge executable.</param>
    /// <param name="cancellationToken">A token that cancels reading the version.</param>
    /// <returns>The version, or <see langword="null"/> if it cannot be read.</returns>
    public override Task<string?> GetInstalledBrowserVersionAsync(string executablePath, CancellationToken cancellationToken)
    {
        return InstalledBrowserVersion.ReadAsync(executablePath, cancellationToken);
    }

    /// <summary>
    /// Gets the msedgedriver version that must be used, which is the version of the installed Edge.
    /// </summary>
    /// <param name="browserVersion">The version of the located Edge, or <see langword="null"/> if it could not be read.</param>
    /// <returns>The required driver version.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the version of Edge is not known.</exception>
    public override string? GetRequiredDriverVersion(string? browserVersion)
    {
        return browserVersion ?? throw new InvalidOperationException($"The version of {this.BrowserDisplayName} could not be read, so the msedgedriver that matches it is not known. Check that Edge is installed, or set {this.DriverEnvironmentVariableName} to a driver.");
    }

    /// <summary>
    /// Gets the download of the msedgedriver that matches the installed Edge.
    /// </summary>
    /// <param name="browserVersion">The version of the located Edge, or <see langword="null"/> if it could not be read.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>A task whose result is the driver download information.</returns>
    public override Task<DriverDownloadInfo> GetMatchingDriverDownloadInfo(string? browserVersion, CancellationToken cancellationToken)
    {
        string version = this.GetRequiredDriverVersion(browserVersion)!;
        string fileName = $"edgedriver_{this.GetDriverPlatformIdentifier()}.zip";
        return Task.FromResult(new DriverDownloadInfo()
        {
            DriverName = DriverNameValue,
            Version = version,
            BrowserVersion = version,
            DownloadUrl = new Uri(this.DownloadOptions.EdgeDriverEndpoint, $"{version}/{fileName}").AbsoluteUri,
            InstallerFileName = fileName,
        });
    }

    private string GetDriverPlatformIdentifier()
    {
        return (this.Platform.OperatingSystem, this.Platform.Architecture) switch
        {
            (OperatingSystemFamily.Windows, Architecture.X64) => "win64",
            (OperatingSystemFamily.Windows, Architecture.X86) => "win32",
            (OperatingSystemFamily.Windows, Architecture.Arm64) => "arm64",
            (OperatingSystemFamily.MacOS, Architecture.X64) => "mac64",
            (OperatingSystemFamily.MacOS, Architecture.Arm64) => "mac64_m1",
            (OperatingSystemFamily.Linux, Architecture.X64) => "linux64",
            _ => throw new PlatformNotSupportedException($"Microsoft publishes no msedgedriver for {this.Platform}."),
        };
    }

    private string GetDefaultSystemInstalledLocation()
    {
        if (this.Platform.OperatingSystem == OperatingSystemFamily.MacOS)
        {
            string applicationBundleName = this.channelValue == EdgeChannel.Stable ? "Microsoft Edge" : $"Microsoft Edge {this.channelValue}";
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                $"{applicationBundleName}.app",
                "Contents",
                "MacOS",
                applicationBundleName);
        }

        if (this.Platform.OperatingSystem == OperatingSystemFamily.Linux)
        {
            string installDirectory = this.channelValue == EdgeChannel.Stable ? "msedge" : $"msedge-{this.Channel}";
            return Path.Combine(
                Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))!,
                "opt",
                "microsoft",
                installDirectory,
                "msedge");
        }

        // Canary installs for the user alone, and the other channels for every user of the machine.
        string applicationSubdirectory = this.channelValue switch
        {
            EdgeChannel.Canary => "Edge SxS",
            EdgeChannel.Stable => "Edge",
            _ => $"Edge {this.channelValue}",
        };
        string relativePath = Path.Combine("Microsoft", applicationSubdirectory, "Application", "msedge.exe");
        return this.channelValue == EdgeChannel.Canary
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), relativePath)
            : GetWindowsProgramFilesPath(relativePath);
    }
}
