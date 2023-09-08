// <copyright file="WebDriverBiDiLogLevel.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Represents log levels for the WebDriver Bidi protocol.
/// </summary>
public enum WebDriverBiDiLogLevel
{
    /// <summary>
    /// The trace level.
    /// </summary>
    Trace,

    /// <summary>
    /// The debug level.
    /// </summary>
    Debug,

    /// <summary>
    /// The info level.
    /// </summary>
    Info,

    /// <summary>
    /// The warning level.
    /// </summary>
    Warn,

    /// <summary>
    /// The error level.
    /// </summary>
    Error,

    /// <summary>
    /// The fatal level.
    /// </summary>
    Fatal,

    /// <summary>
    /// Suppresses all logs.
    /// </summary>
    Off,
}