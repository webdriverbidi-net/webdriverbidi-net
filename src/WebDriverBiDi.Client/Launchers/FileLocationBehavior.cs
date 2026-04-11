// <copyright file="FileLocationBehavior.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Specifies how an executable file (browser or driver) should be located.
/// </summary>
public enum FileLocationBehavior
{
    /// <summary>
    /// Use the system-installed executable.
    /// </summary>
    UseSystemInstallLocation,

    /// <summary>
    /// Use a custom path to the executable.
    /// </summary>
    UseCustomLocation,

    /// <summary>
    /// Automatically locate the executable, downloading it if necessary.
    /// </summary>
    AutoLocateAndDownload,
}
