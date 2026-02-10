// <copyright file="CommandResponseMessage{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Text.Json.Serialization;

/// <summary>
/// Base class for the result of a command where the concrete type of the response data is known.
/// </summary>
/// <typeparam name="T">The data type of the command response.</typeparam>
public class CommandResponseMessage<T> : CommandResponseMessage
    where T : CommandResult
{
    /// <summary>
    /// Gets the result of the command.
    /// </summary>
    [JsonIgnore]
    public override CommandResult Result => this.SerializableResult!;

    /// <summary>
    /// Gets the result of the command for serialization purposes.
    /// </summary>
    [JsonPropertyName("result")]
    [JsonRequired]
    [JsonInclude]
    internal T? SerializableResult { get; set; }
}
