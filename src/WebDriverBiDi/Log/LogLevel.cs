// <copyright file="LogLevel.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Log;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The valid log levels for logging in the browser.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<LogLevel>))]
public enum LogLevel
{
    /// <summary>
    /// The debug log level.
    /// </summary>
    Debug,

    /// <summary>
    /// The info log level.
    /// </summary>
    Info,

    /// <summary>
    /// The warning log level.
    /// </summary>
    Warn,

    /// <summary>
    /// The error log level.
    /// </summary>
    Error,
}
