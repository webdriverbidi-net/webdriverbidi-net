// <copyright file="GenericLogEntry.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Log;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a log entry in the browser.
/// </summary>
public class GenericLogEntry : LogEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericLogEntry" /> class.
    /// </summary>
    [JsonConstructor]
    internal GenericLogEntry()
        : base()
    {
    }
}
