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
/// <remarks>
/// <para>
/// This class is public to support AOT (ahead-of-time) compilation scenarios in extension libraries.
/// End users do not construct or reference this type directly; it is used internally by
/// <see cref="CommandParameters{T}"/> to express the expected response type for a command.
/// </para>
/// <para>
/// Authors of third-party extension libraries that add new protocol commands must annotate their
/// own <see cref="System.Text.Json.Serialization.JsonSerializerContext"/> with
/// <c>[JsonSerializable(typeof(CommandResponseMessage&lt;TResult&gt;))]</c> for each of their
/// custom result types, then register the context via
/// <see cref="WebDriverBiDi.Protocol.Transport.RegisterTypeInfoResolverAsync"/>. This pattern mirrors
/// how the library itself registers all built-in command response types in
/// <see cref="WebDriverBiDi.JsonConverters.WebDriverBiDiJsonSerializerContext"/>.
/// </para>
/// </remarks>
public class CommandResponseMessage<T> : CommandResponseMessage
    where T : CommandResult
{
    /// <summary>
    /// Gets the result of the command.
    /// </summary>
    [JsonIgnore]
    public override CommandResult Result => this.SerializableResult ?? throw new InvalidOperationException("Result cannot be null");

    /// <summary>
    /// Gets or sets the result of the command for serialization purposes.
    /// </summary>
    [JsonPropertyName("result")]
    [JsonRequired]
    [JsonInclude]
    internal T? SerializableResult { get; set; }
}
