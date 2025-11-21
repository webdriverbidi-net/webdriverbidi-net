// <copyright file="LogMessageEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Object containing event data for events raised when a log message is received from a WebDriver Bidi connection.
/// </summary>
public record LogMessageEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogMessageEventArgs" /> class with a log message, log level,
    /// and name of component from which the message is being logged.
    /// </summary>
    /// <param name="message">The message sent to the log.</param>
    /// <param name="level">The log level of the message sent to the log.</param>
    /// <param name="componentName">The name of the component from which the message is being logged.</param>
    public LogMessageEventArgs(string message, WebDriverBiDiLogLevel level, string componentName)
    {
        this.Message = message;
        this.Level = level;
        this.ComponentName = componentName;
    }

    /// <summary>
    /// Gets the text of the message sent to the log.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the log level of the message sent to the log.
    /// </summary>
    public WebDriverBiDiLogLevel Level { get; }

    /// <summary>
    /// Gets the component name from which this log message originates.
    /// </summary>
    public string ComponentName { get; } = string.Empty;

    /// <summary>
    /// Gets the date and time (in UTC) when this log entry was created.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
