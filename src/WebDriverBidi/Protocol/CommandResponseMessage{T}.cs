// <copyright file="CommandResponseMessage{T}.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Protocol;

using Newtonsoft.Json;

/// <summary>
/// Base class for the result of a command where the concrete type of the response data is known.
/// </summary>
/// <typeparam name="T">The data type of the command response.</typeparam>
[JsonObject(MemberSerialization.OptIn)]
public class CommandResponseMessage<T> : CommandResponseMessage
    where T : CommandResult
{
    private T? result;

    /// <summary>
    /// Gets the result of the command.
    /// </summary>
    public override CommandResult Result => this.result!;

    /// <summary>
    /// Gets the result of the command for serialization purposes.
    /// </summary>
    [JsonProperty("result")]
    [JsonRequired]
    internal T? SerializableResult { get => this.result; private set => this.result = value; }
}