// <copyright file="BrowserLauncherProcessStartedEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Client;

using System;
using System.Diagnostics;
using System.IO;

/// <summary>
/// Provides data for the LauncherProcessStarted event of a <see cref="BrowserLauncher"/> object.
/// </summary>
public class BrowserLauncherProcessStartedEventArgs : EventArgs
{
    private readonly int processId;
    private readonly StreamReader? standardOutputStreamReader;
    private readonly StreamReader? standardErrorStreamReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLauncherProcessStartedEvetArgs"/> class.
    /// </summary>
    /// <param name="launcherProcess">The <see cref="Process"/> object started.</param>
    public BrowserLauncherProcessStartedEventArgs(Process launcherProcess)
    {
        this.processId = launcherProcess.Id;
        if (launcherProcess.StartInfo.RedirectStandardOutput && !launcherProcess.StartInfo.UseShellExecute)
        {
            this.standardOutputStreamReader = launcherProcess.StandardOutput;
        }

        if (launcherProcess.StartInfo.RedirectStandardError && !launcherProcess.StartInfo.UseShellExecute)
        {
            this.standardErrorStreamReader = launcherProcess.StandardError;
        }
    }

    /// <summary>
    /// Gets the unique ID of the driver executable process.
    /// </summary>
    public int ProcessId
    {
        get { return this.processId; }
    }

    /// <summary>
    /// Gets a <see cref="StreamReader"/> object that can be used to read the contents
    /// printed to stdout by a driver service process.
    /// </summary>
    public StreamReader? StandardOutputStreamReader
    {
        get { return this.standardOutputStreamReader; }
    }

    /// <summary>
    /// Gets a <see cref="StreamReader"/> object that can be used to read the contents
    /// printed to stderr by a driver service process.
    /// </summary>
    public StreamReader? StandardErrorStreamReader
    {
        get { return standardErrorStreamReader; }
    }
}

