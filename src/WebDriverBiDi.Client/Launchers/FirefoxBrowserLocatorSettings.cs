// <copyright file="FirefoxBrowserLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Runtime.InteropServices;

/// <summary>
/// Defines the locator settings for the Firefox browser, including properties such as the browser name,
/// channel, version, environment variable name, expected executable path, and installer file name.
/// This class also includes methods to retrieve browser download information based on the specified
/// channel or version, and to determine the appropriate download URL for the Firefox browser based
/// on the current platform and architecture. The locator settings are used by the BrowserLocator to
/// locate and download the correct version of Firefox for testing with WebDriver BiDi.
/// </summary>
public class FirefoxBrowserLocatorSettings : BrowserLocatorSettings
{
    private readonly FirefoxChannel channelValue;
    private readonly string versionValue = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirefoxBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="version">The specific released version of the Firefox browser.</param>
    public FirefoxBrowserLocatorSettings(string version)
        : this(FirefoxChannel.Stable, $"Firefox {version}")
    {
        this.versionValue = version;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FirefoxBrowserLocatorSettings"/> class.
    /// </summary>
    /// <param name="channel">The distribution channel of the Firefox browser.</param>
    public FirefoxBrowserLocatorSettings(FirefoxChannel channel)
        : this(channel, $"Firefox {channel}", true)
    {
    }

    private FirefoxBrowserLocatorSettings(FirefoxChannel channel, string browserDisplayName, bool isDefaultChannelVersion = false)
    {
        this.channelValue = channel;
        this.BrowserName = "firefox";
        this.Channel = channel.ToString().ToLowerInvariant();
        this.BrowserDisplayName = browserDisplayName;
        this.IsLatestChannelVersion = isDefaultChannelVersion;
        this.EnvironmentVariableName = "FIREFOX_EXECUTABLE";
        this.Extractor = this.InitializeExtractor();
        this.InstallerFileName = this.InitializeInstallerFileName();
        this.ExpectedExecutablePath = this.InitializeExpectedExecutablePath();
    }

    /// <summary>
    /// Gets the browser download information for the Firefox browser version or channel specified..
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the browser download information as the result.</returns>
    public override async Task<BrowserDownloadInfo> GetBrowserDownloadInfo()
    {
        BrowserDownloadInfo downloadInfo = new BrowserDownloadInfo
        {
            BrowserName = this.BrowserName,
            Channel = this.Channel,
            Version = this.versionValue,
        };

        if (!string.IsNullOrEmpty(this.versionValue))
        {
            downloadInfo.DownloadUrl = this.GetDirectDownloadUrl();
            return downloadInfo;
        }

        string downloadServiceUrl = this.GetDownloadServiceUrl();

        HttpClientHandler handler = new()
        {
            AllowAutoRedirect = false,
        };
        using HttpClient httpClient = new(handler);
        HttpResponseMessage response = await httpClient.GetAsync(downloadServiceUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        if (response.Headers.Location is not null)
        {
            string location = response.Headers.Location.ToString();
            downloadInfo.DownloadUrl = location;
            downloadInfo.Version = this.GetVersionNumberFromDownloadUrl(location);
            downloadInfo.IgnoreVersionMatch = this.channelValue == FirefoxChannel.Nightly;
            return downloadInfo;
        }

        downloadInfo.DownloadUrl = downloadServiceUrl;
        return downloadInfo;
    }

    private BrowserExtractor InitializeExtractor()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new DiskImageBrowserExtractor();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new TarballBrowserExtractor();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new SelfExtractingExecutableBrowserExtractor();
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
        }
    }

    private string InitializeInstallerFileName()
    {
        string baseName = $"{this.BrowserName}-{this.Channel}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"{baseName}.dmg";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return $"{baseName}.tar.xz";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $"{baseName}-installer.exe";
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
    }

    private string InitializeExpectedExecutablePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string appBundleName = "Firefox.app";
            if (this.channelValue == FirefoxChannel.Dev)
            {
                appBundleName = "Firefox Developer Edition.app";
            }
            else if (this.channelValue == FirefoxChannel.Nightly)
            {
                appBundleName = "Firefox Nightly.app";
            }

            return Path.Combine(appBundleName, "Contents", "MacOS", "firefox");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine("firefox", "firefox");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine("firefox", "firefox.exe");
        }

        throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
    }

    private string GetVersionNumberFromDownloadUrl(string downloadUrl)
    {
        string versionStartMarker = this.channelValue == FirefoxChannel.Nightly ? "firefox-" : "releases/";
        string versionEndMarker = this.channelValue == FirefoxChannel.Nightly ? ".en-US" : "/";

        int versionStartIndex = downloadUrl.IndexOf(versionStartMarker) + versionStartMarker.Length;
        int versionEndIndex = downloadUrl.IndexOf(versionEndMarker, versionStartIndex);
        return downloadUrl.Substring(versionStartIndex, versionEndIndex - versionStartIndex);
    }

    private string GetDirectDownloadUrl()
    {
        string osMarker;
        string fileName;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            osMarker = "mac";
            fileName = $"Firefox {this.versionValue}.dmg";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            osMarker = "linux-x86_64";
            fileName = $"firefox-{this.versionValue}.tar.xz";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            osMarker = "win64";
            fileName = $"Firefox Setup {this.versionValue}.exe";
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
        }

        return $"https://download-installer.cdn.mozilla.net/pub/firefox/releases/{this.versionValue}/{osMarker}/en-US/{fileName}";
    }

    private string GetDownloadServiceUrl()
    {
        string osMarker;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            osMarker = "osx";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            osMarker = "linux64";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            osMarker = "win64";
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform for {this.BrowserDisplayName} download.");
        }

        string product = this.channelValue switch
        {
            FirefoxChannel.Stable => "firefox-latest",
            FirefoxChannel.Beta => "firefox-beta-latest",
            FirefoxChannel.Dev => "firefox-devedition-latest",
            FirefoxChannel.Nightly => "firefox-nightly-latest",
            _ => throw new InvalidOperationException($"Unsupported Firefox channel: {this.channelValue}."),
        };

        return $"https://download.mozilla.org/?product={product}-ssl&os={osMarker}&lang=en-US";
    }
}
