// <copyright file="StatusCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Result for getting the status of a remote end using the session.status command.
/// </summary>
public class StatusCommandResult : CommandResult
{
    private bool ready;
    private string message = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusCommandResult"/> class.
    /// </summary>
    [JsonConstructor]
    public StatusCommandResult()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the remote end is able to create new sessions.
    /// </summary>
    [JsonPropertyName("ready")]
    [JsonRequired]
    [JsonInclude]
    public bool IsReady { get => this.ready; internal set => this.ready = value; }

    /// <summary>
    /// Gets a message about the status from the remote end.
    /// </summary>
    [JsonPropertyName("message")]
    [JsonRequired]
    [JsonInclude]
    public string Message { get => this.message; internal set => this.message = value; }
}