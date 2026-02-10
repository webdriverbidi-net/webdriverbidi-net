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
    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry" /> class.
    /// </summary>
    internal LogEntry()
    {
        this.EpochTimestamp = -1;
    }

    /// <summary>
    /// Gets the type of log entry.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonRequired]
    [JsonInclude]
    public string Type { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the log level of the log entry.
    /// </summary>
    [JsonPropertyName("level")]
    [JsonRequired]
    [JsonInclude]
    public LogLevel Level { get; internal set; } = LogLevel.Error;

    /// <summary>
    /// Gets the source of the log entry.
    /// </summary>
    [JsonPropertyName("source")]
    [JsonRequired]
    [JsonInclude]
    public Source Source { get; internal set; } = new();

    /// <summary>
    /// Gets the text of the log entry.
    /// </summary>
    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? Text { get; internal set; }

    /// <summary>
    /// Gets the stack trace of the log entry.
    /// </summary>
    [JsonPropertyName("stackTrace")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public StackTrace? StackTrace { get; internal set; }

    /// <summary>
    /// Gets the timestamp in UTC of the log entry.
    /// </summary>
    [JsonIgnore]
    public DateTime Timestamp { get; internal set; }

    /// <summary>
    /// Gets the timestamp as the total number of milliseconds elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonPropertyName("timestamp")]
    [JsonRequired]
    [JsonInclude]
    public long EpochTimestamp
    {
        get;
        internal set
        {
            field = value;
            this.Timestamp = DateTimeUtilities.UnixEpoch.AddMilliseconds(value);
        }
    }
}
