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
