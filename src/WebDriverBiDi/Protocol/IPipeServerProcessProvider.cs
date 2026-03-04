// <copyright file="IPipeServerProcessProvider.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Diagnostics;

/// <summary>
/// Defines an interface for providing a process that hosts a pipe server. This allows for
/// dependency injection and testing of components that rely on a pipe server process.
/// </summary>
/// <remarks>
/// <para>
/// This interface is used by <see cref="PipeConnection"/> to manage the browser process lifecycle
/// when using anonymous pipes for communication. Implementations are responsible for starting
/// the browser process with the <c>--remote-debugging-pipe</c> flag and providing pipe handles
/// for communication.
/// </para>
/// <para>
/// The provided process must:
/// <list type="bullet">
/// <item><description>Be started before <see cref="PipeConnection.StartAsync"/> is called</description></item>
/// <item><description>Accept pipe handles via command-line arguments or environment variables</description></item>
/// <item><description>Support the null-terminated JSON message protocol</description></item>
/// <item><description>Read from file descriptor/handle for incoming messages</description></item>
/// <item><description>Write to file descriptor/handle for outgoing messages</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Note:</strong> This interface is rarely needed by most users. Browser launchers
/// that support pipe connections typically implement this interface internally.
/// </para>
/// </remarks>
public interface IPipeServerProcessProvider
{
    /// <summary>
    /// Gets the process that hosts the pipe server. This process is
    /// expected to be started and running when accessed. The pipe
    /// server process should be responsible for managing the lifetime
    /// of the pipe handles used for communication.
    /// </summary>
    Process? PipeServerProcess { get; }
}
