// <copyright file="BrowserLauncherProcessStartedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System;
using System.Diagnostics;
using System.IO;

/// <summary>
/// Provides data for the LauncherProcessStarted event of a <see cref="BrowserLauncher"/> object.
/// </summary>
public record BrowserLauncherProcessStartedEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncherProcessStartedEventArgs"/> class.
    /// </summary>
    /// <param name="launcherProcess">The <see cref="Process"/> object started.</param>
    public BrowserLauncherProcessStartedEventArgs(Process launcherProcess)
    {
        this.ProcessId = launcherProcess.Id;
        if (launcherProcess.StartInfo.RedirectStandardOutput && !launcherProcess.StartInfo.UseShellExecute)
        {
            this.StandardOutputStreamReader = launcherProcess.StandardOutput;
        }

        if (launcherProcess.StartInfo.RedirectStandardError && !launcherProcess.StartInfo.UseShellExecute)
        {
            this.StandardErrorStreamReader = launcherProcess.StandardError;
        }
    }

    /// <summary>
    /// Gets the unique ID of the driver executable process.
    /// </summary>
    public int ProcessId { get; }

    /// <summary>
    /// Gets a <see cref="StreamReader"/> object that can be used to read the contents
    /// printed to stdout by a driver service process.
    /// </summary>
    public StreamReader? StandardOutputStreamReader { get; }

    /// <summary>
    /// Gets a <see cref="StreamReader"/> object that can be used to read the contents
    /// printed to stderr by a driver service process.
    /// </summary>
    public StreamReader? StandardErrorStreamReader { get; }
}
