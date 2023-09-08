// <copyright file="LogEntry.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Log;

using Newtonsoft.Json;
using WebDriverBiDi.Internal;
using WebDriverBiDi.JsonConverters;
using WebDriverBiDi.Script;

/// <summary>
/// Represents a log entry in the browser.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(LogEntryJsonConverter))]
public class LogEntry
{
    private string type = string.Empty;
    private LogLevel level = LogLevel.Error;
    private Source source = new();
    private string? text;
    private long epochTimestamp = -1;
    private DateTime timestamp;
    private StackTrace? stacktrace;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry" /> class.
    /// </summary>
    internal LogEntry()
    {
    }

    /// <summary>
    /// Gets the type of log entry.
    /// </summary>
    [JsonProperty("type")]
    [JsonRequired]
    public string Type { get => this.type; internal set => this.type = value; }

    /// <summary>
    /// Gets the log level of the log entry.
    /// </summary>
    [JsonProperty("level")]
    [JsonRequired]
    public LogLevel Level { get => this.level; internal set => this.level = value; }

    /// <summary>
    /// Gets the source of the log entry.
    /// </summary>
    [JsonProperty("source")]
    [JsonRequired]
    public Source Source { get => this.source; internal set => this.source = value; }

    /// <summary>
    /// Gets the text of the log entry.
    /// </summary>
    [JsonProperty("text", NullValueHandling = NullValueHandling.Include)]
    public string? Text { get => this.text; internal set => this.text = value; }

    /// <summary>
    /// Gets the stack trace of the log entry.
    /// </summary>
    [JsonProperty("stacktrace", NullValueHandling = NullValueHandling.Ignore)]
    public StackTrace? StackTrace { get => this.stacktrace; internal set => this.stacktrace = value; }

    /// <summary>
    /// Gets the timestamp in UTC of the log entry.
    /// </summary>
    public DateTime Timestamp => this.timestamp;

    /// <summary>
    /// Gets the timestamp as the total number of milliseconds elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonProperty("timestamp")]
    [JsonRequired]
    public long EpochTimestamp
    {
        get
        {
            return this.epochTimestamp;
        }

        private set
        {
            this.epochTimestamp = value;
            this.timestamp = DateTimeUtilities.UnixEpoch.AddMilliseconds(value);
        }
    }
}
