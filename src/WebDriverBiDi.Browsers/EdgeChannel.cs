// <copyright file="EdgeChannel.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// The distribution channels of Microsoft Edge, each of which installs separately.
/// </summary>
internal enum EdgeChannel
{
    /// <summary>
    /// The Stable channel.
    /// </summary>
    Stable,

    /// <summary>
    /// The Beta channel.
    /// </summary>
    Beta,

    /// <summary>
    /// The Dev channel.
    /// </summary>
    Dev,

    /// <summary>
    /// The Canary channel.
    /// </summary>
    Canary,
}
