// <copyright file="BrowserLauncherProcessStartingEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Client;

using System;
using System.Diagnostics;

/// <summary>
/// Provides data for the LauncherProcessStarting event of a <see cref="BrowserLauncher"/> object.
/// </summary>
public class BrowserLauncherProcessStartingEventArgs : EventArgs
{
    private readonly ProcessStartInfo startInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncherProcessStartingEventArgs"/> class.
    /// </summary>
    /// <param name="startInfo">The <see cref="ProcessStartInfo"/> of the
    /// driver process to be started.</param>
    public BrowserLauncherProcessStartingEventArgs(ProcessStartInfo startInfo)
    {
        this.startInfo = startInfo;
    }

    /// <summary>
    /// Gets the <see cref="ProcessStartInfo"/> object with which the
    /// driver service process will be started.
    /// </summary>
    public ProcessStartInfo DriverServiceProcessStartInfo
    {
        get { return this.startInfo; }
    }
}
