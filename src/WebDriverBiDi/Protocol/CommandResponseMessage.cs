// <copyright file="CommandResponseMessage.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Text.Json.Serialization;

/// <summary>
/// Base class for the result of a command.
/// </summary>
public abstract class CommandResponseMessage : Message
{
    /// <summary>
    /// Gets the ID for this command during execution.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonInclude]
    [JsonRequired]
    public long Id { get; internal set; }

    /// <summary>
    /// Gets the result data for the command.
    /// </summary>
    [JsonIgnore]
    public abstract CommandResult Result { get; }
}
