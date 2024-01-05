// <copyright file="LogEntry.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Log;

using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;
using WebDriverBiDi.JsonConverters;
using WebDriverBiDi.Script;

/// <summary>
/// Represents a log entry in the browser.
/// </summary>
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
    [JsonPropertyName("type")]
    [JsonRequired]
    [JsonInclude]
    public string Type { get => this.type; internal set => this.type = value; }

    /// <summary>
    /// Gets the log level of the log entry.
    /// </summary>
    [JsonPropertyName("level")]
    [JsonRequired]
    [JsonInclude]
    public LogLevel Level { get => this.level; internal set => this.level = value; }

    /// <summary>
    /// Gets the source of the log entry.
    /// </summary>
    [JsonPropertyName("source")]
    [JsonRequired]
    [JsonInclude]
    public Source Source { get => this.source; internal set => this.source = value; }

    /// <summary>
    /// Gets the text of the log entry.
    /// </summary>
    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? Text { get => this.text; internal set => this.text = value; }

    /// <summary>
    /// Gets the stack trace of the log entry.
    /// </summary>
    [JsonPropertyName("stackTrace")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public StackTrace? StackTrace { get => this.stacktrace; internal set => this.stacktrace = value; }

    /// <summary>
    /// Gets the timestamp in UTC of the log entry.
    /// </summary>
    [JsonIgnore]
    public DateTime Timestamp => this.timestamp;

    /// <summary>
    /// Gets the timestamp as the total number of milliseconds elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonPropertyName("timestamp")]
    [JsonRequired]
    [JsonInclude]
    public long EpochTimestamp
    {
        get
        {
            return this.epochTimestamp;
        }

        internal set
        {
            this.epochTimestamp = value;
            this.timestamp = DateTimeUtilities.UnixEpoch.AddMilliseconds(value);
        }
    }
}
