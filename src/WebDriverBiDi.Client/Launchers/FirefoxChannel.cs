// <copyright file="FirefoxChannel.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Defines the distribution channels for the Firefox browser, which include Stable, Beta, Dev, and Nightly.
/// </summary>
public enum FirefoxChannel
{
    /// <summary>
    /// The Stable channel, which is the default channel for released versions of
    /// Firefox that have been thoroughly tested and are considered stable for
    /// general use.
    /// </summary>
    Stable,

    /// <summary>
    /// The Beta channel, which includes versions of Firefox that are in the final stages
    /// of testing before release. These versions may contain new features and bug fixes
    /// that are not yet available in the Stable channel, but they may also have some
    /// instability or issues that are still being addressed.
    /// </summary>
    Beta,

    /// <summary>
    /// The Dev channel, which includes versions of Firefox that are in active development and testing.
    /// These versions may contain experimental features and changes that are not yet fully tested,
    /// and they may be more likely to have bugs or issues compared to the Stable and Beta channels.
    /// </summary>
    Dev,

    /// <summary>
    /// The Nightly channel, which includes the latest builds of Firefox that are updated often with
    /// the newest features and changes. These versions are the least stable and are intended for
    /// developers and early adopters who want to test the latest features and provide feedback to
    /// the Firefox development team. Nightly builds may have significant bugs and issues, and they
    /// are not recommended for general use.
    /// </summary>
    Nightly,
}
