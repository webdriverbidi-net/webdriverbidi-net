// <copyright file="BrowserReleaseChannel.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Defines the browser-independent release channels for browsers that can be used for testing.
/// These channels represent different stages of the browser release cycle, with varying levels
/// of stability and access to new features. The specific channel names may vary between
/// different browsers, but common channels include stable, beta, developer preview, and alpha
/// (or canary/nightly) releases.
/// </summary>
public enum BrowserReleaseChannel
{
    /// <summary>
    /// The browser stable release channel, which is the default channel for end users
    /// and is the most thoroughly tested and stable version of the browser.
    /// </summary>
    Stable,

    /// <summary>
    /// The developer preview release channel, which provides early access to new features and improvements.
    /// </summary>
    DeveloperPreview,

    /// <summary>
    /// The beta release channel, which is intended for users who want to test new features
    /// before they are released in the stable channel.
    /// </summary>
    Beta,

    /// <summary>
    /// The alpha release channel, which is intended for users who want to test the most
    /// recent features and improvements. This channel has different names for different
    /// browsers (e.g., "canary" for Chrome, "nightly" for Firefox).
    /// </summary>
    Alpha,
}
