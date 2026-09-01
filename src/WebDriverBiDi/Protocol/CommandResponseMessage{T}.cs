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
/// Authors of third-party extension libraries that add new protocol commands must not register
/// this envelope type in their own
/// <see cref="System.Text.Json.Serialization.JsonSerializerContext"/>: its serializable members
/// are internal to this library, so a context in another assembly cannot generate working
/// metadata for it. The transport builds the envelope's type info itself (see
/// <see cref="CommandParameters{T}"/>) and asks the serializer options only for the
/// <typeparamref name="T"/> result type. Annotate your context with
/// <c>[JsonSerializable(typeof(TResult))]</c> for each custom result type, then register the
/// context via <see cref="WebDriverBiDi.Protocol.Transport.RegisterTypeInfoResolverAsync"/>.
/// See the AOT compatibility article in the library documentation for the full pattern.
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
