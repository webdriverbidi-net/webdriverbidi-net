// <copyright file="EntryAddedEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Log;

using Newtonsoft.Json;
using WebDriverBidi.Script;

/// <summary>
/// Object containing event data for the event raised when a log entry is added.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class EntryAddedEventArgs : WebDriverBidiEventArgs
{
    private readonly LogEntry entry;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryAddedEventArgs" /> class.
    /// </summary>
    /// <param name="entry">The data describing the log entry.</param>
    public EntryAddedEventArgs(LogEntry entry)
    {
        this.entry = entry;
    }

    /// <summary>
    /// Gets the type of log entry.
    /// </summary>
    public string Type => this.entry.Type;

    /// <summary>
    /// Gets the source of the log entry.
    /// </summary>
    public Source Source => this.entry.Source;

    /// <summary>
    /// Gets the text of the log entry.
    /// </summary>
    public string? Text => this.entry.Text;

    /// <summary>
    /// Gets the timestamp of the log entry.
    /// </summary>
    public DateTime Timestamp => this.entry.Timestamp;

    /// <summary>
    /// Gets the stack trace of the log entry.
    /// </summary>
    public StackTrace? StackTrace => this.entry.StackTrace;

    /// <summary>
    /// Gets the method name of the log entry.
    /// </summary>
    public string? Method
    {
        get
        {
            if (this.entry is not ConsoleLogEntry consoleLogEntry)
            {
                return null;
            }

            return consoleLogEntry.Method;
        }
    }

    /// <summary>
    /// Gets the read-only list of arguments for the log entry.
    /// </summary>
    public IList<RemoteValue>? Arguments
    {
        get
        {
            if (this.entry is not ConsoleLogEntry consoleLogEntry)
            {
                return null;
            }

            return consoleLogEntry.Args;
        }
    }
}