// <copyright file="EventMessageRegistration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Describes how the <see cref="Transport"/> deserializes a registered event: the message type,
/// and an optional factory that builds its <see cref="JsonTypeInfo"/> without consulting the
/// serializer's type info resolvers for the envelope.
/// </summary>
internal sealed class EventMessageRegistration
{
    private readonly Func<JsonSerializerOptions, JsonTypeInfo>? typeInfoFactory;
    private JsonTypeInfo? typeInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventMessageRegistration"/> class.
    /// </summary>
    /// <param name="eventMessageType">The type the event message deserializes to.</param>
    /// <param name="typeInfoFactory">A factory creating the type info for <paramref name="eventMessageType"/>, or <see langword="null"/> to resolve it through the serializer options.</param>
    public EventMessageRegistration(Type eventMessageType, Func<JsonSerializerOptions, JsonTypeInfo>? typeInfoFactory)
    {
        this.EventMessageType = eventMessageType;
        this.typeInfoFactory = typeInfoFactory;
    }

    /// <summary>
    /// Gets the type the event message deserializes to.
    /// </summary>
    public Type EventMessageType { get; }

    /// <summary>
    /// Gets the type info used to deserialize the event message, creating it on first use.
    /// </summary>
    /// <param name="options">The serializer options in effect.</param>
    /// <returns>The type info.</returns>
    public JsonTypeInfo GetTypeInfo(JsonSerializerOptions options)
    {
        // Creation is idempotent, so a benign race between message-processing threads is harmless.
        return this.typeInfo ??= this.typeInfoFactory?.Invoke(options) ?? options.GetTypeInfo(this.EventMessageType);
    }
}
