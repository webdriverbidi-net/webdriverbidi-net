// <copyright file="JavaScriptLogEntry.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Log;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a log entry in the browser.
/// </summary>
public class JavaScriptLogEntry : LogEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaScriptLogEntry" /> class.
    /// </summary>
    [JsonConstructor]
    internal JavaScriptLogEntry()
        : base()
    {
    }
}
