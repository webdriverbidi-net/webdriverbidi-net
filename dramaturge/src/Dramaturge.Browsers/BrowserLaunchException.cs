// <copyright file="BrowserLaunchException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

using WebDriverBiDi;

/// <summary>
/// The exception that is thrown when a browser, or the driver or remote end that launches it, cannot be started.
/// </summary>
public class BrowserLaunchException : WebDriverBiDiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLaunchException"/> class.
    /// </summary>
    public BrowserLaunchException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLaunchException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BrowserLaunchException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLaunchException"/> class with a specified error
    /// message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BrowserLaunchException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLaunchException"/> class for a launched
    /// process that exited or never became ready.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="exitCode">The exit code of the process, or <see langword="null"/> if it is still running.</param>
    /// <param name="processOutput">The last lines the process wrote to its standard output and error streams.</param>
    public BrowserLaunchException(string message, int? exitCode, IReadOnlyList<string> processOutput)
        : base(processOutput.Count == 0 ? message : $"{message}{Environment.NewLine}Last output of the process:{Environment.NewLine}{string.Join(Environment.NewLine, processOutput)}")
    {
        this.ExitCode = exitCode;
        this.ProcessOutput = processOutput;
    }

    /// <summary>
    /// Gets the exit code of the launched process, or <see langword="null"/> if it did not exit or none was launched.
    /// </summary>
    public int? ExitCode { get; }

    /// <summary>
    /// Gets the last lines the launched process wrote to its standard output and error streams.
    /// </summary>
    public IReadOnlyList<string> ProcessOutput { get; } = [];
}
