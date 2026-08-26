// <copyright file="CommandParameters{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.JsonConverters;
using WebDriverBiDi.Protocol;

/// <summary>
/// Represents data for a WebDriver Bidi command where the response type is known.
/// </summary>
/// <typeparam name="T">The type of the response for this command.</typeparam>
public abstract class CommandParameters<T> : CommandParameters
    where T : CommandResult
{
    /// <summary>
    /// Gets the type of the response for this command.
    /// </summary>
    [JsonIgnore]
    public override Type ResponseType => typeof(CommandResponseMessage<T>);

    /// <summary>
    /// Creates the type info for <see cref="CommandResponseMessage{T}"/> using the library-owned envelope
    /// converter, so that only <typeparamref name="T"/> needs to be resolvable through the serializer options.
    /// </summary>
    /// <param name="options">The serializer options in effect for the transport.</param>
    /// <returns>The type info for the response envelope.</returns>
    /// <remarks>
    /// <typeparamref name="T"/> is a compile-time generic argument here, so this instantiation is
    /// ahead-of-time compatible; no reflection over open generic types is involved.
    /// </remarks>
    internal override JsonTypeInfo CreateResponseTypeInfo(JsonSerializerOptions options)
    {
        return JsonMetadataServices.CreateValueInfo<CommandResponseMessage<T>>(options, new CommandResponseMessageJsonConverter<T>());
    }
}
