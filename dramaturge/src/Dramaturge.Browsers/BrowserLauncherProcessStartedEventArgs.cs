// <copyright file="BrowserLauncherProcessStartedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using System.Diagnostics;
using WebDriverBiDi;

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

        // A process whose output is redirected cannot have been started by the shell.
        if (launcherProcess.StartInfo.RedirectStandardOutput)
        {
            this.StandardOutputStreamReader = launcherProcess.StandardOutput;
        }

        if (launcherProcess.StartInfo.RedirectStandardError)
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
