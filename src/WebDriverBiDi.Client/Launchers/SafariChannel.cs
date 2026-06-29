// <copyright file="SafariChannel.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Defines the distribution channels for the Safari browser, which include Stable and Technology Preview.
/// </summary>
public enum SafariChannel
{
    /// <summary>
    /// The Stable channel, which is the default channel for released versions of
    /// Safari that have been thoroughly tested and are considered stable for
    /// general use.
    /// </summary>
    Stable,

    /// <summary>
    /// The Technology Preview channel, which includes versions of Safari that are in the
    /// final stages of testing before release. These versions may contain new features and
    /// bug fixes that are not yet available in the Stable channel, but they may also have
    /// some instability or issues that are still being addressed.
    /// </summary>
    TechnologyPreview,
}
