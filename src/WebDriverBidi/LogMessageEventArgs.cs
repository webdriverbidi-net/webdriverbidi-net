// <copyright file="LogMessageEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Object containing event data for events raised when a log message is received from a WebDriver Bidi connection.
/// </summary>
public class LogMessageEventArgs : WebDriverBidiEventArgs
{
    private readonly string message;
    private readonly WebDriverBidiLogLevel level;
    private readonly string componentName = string.Empty;
    private readonly DateTime timestamp = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogMessageEventArgs" /> class with a log message, log level,
    /// and name of component from which the message is being logged.
    /// </summary>
    /// <param name="message">The message sent to the log.</param>
    /// <param name="level">The log level of the message sent to the log.</param>
    /// <param name="componentName">The name of the component from which the message is being logged.</param>
    public LogMessageEventArgs(string message, WebDriverBidiLogLevel level, string componentName)
    {
        this.message = message;
        this.level = level;
        this.componentName = componentName;
    }

    /// <summary>
    /// Gets the text of the message sent to the log.
    /// </summary>
    public string Message => this.message;

    /// <summary>
    /// Gets the log level of the message sent to the log.
    /// </summary>
    public WebDriverBidiLogLevel Level => this.level;

    /// <summary>
    /// Gets the component name from which this log message originates.
    /// </summary>
    public string ComponentName => this.componentName;

    /// <summary>
    /// Gets the date and time (in UTC) when this log entry was created.
    /// </summary>
    public DateTime Timestamp => this.timestamp;
}