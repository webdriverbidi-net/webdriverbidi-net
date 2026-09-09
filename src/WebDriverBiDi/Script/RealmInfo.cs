// <copyright file="RealmInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing information about a script realm.
/// </summary>
[JsonConverter(typeof(DiscriminatedUnionJsonConverter<RealmInfo>))]
[DiscriminatedTypeProperty("type")]
[DiscriminatedDerivedType(typeof(WindowRealmInfo), "window")]
[DiscriminatedDerivedType(typeof(DedicatedWorkerRealmInfo), "dedicated-worker")]
[DiscriminatedDerivedType(typeof(SharedWorkerRealmInfo), "shared-worker")]
[DiscriminatedDerivedType(typeof(ServiceWorkerRealmInfo), "service-worker")]
[DiscriminatedDerivedType(typeof(WorkerRealmInfo), "worker")]
[DiscriminatedDerivedType(typeof(PaintWorkletRealmInfo), "paint-worklet")]
[DiscriminatedDerivedType(typeof(AudioWorkletRealmInfo), "audio-worklet")]
[DiscriminatedDerivedType(typeof(WorkletRealmInfo), "worklet")]
public abstract record RealmInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RealmInfo"/> class.
    /// </summary>
    internal RealmInfo()
    {
    }

    /// <summary>
    /// Gets the ID of the realm.
    /// </summary>
    [JsonPropertyName("realm")]
    [JsonRequired]
    [JsonInclude]
    public string RealmId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the origin of the realm.
    /// </summary>
    [JsonPropertyName("origin")]
    [JsonRequired]
    [JsonInclude]
    public string Origin { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the type of the realm.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonRequired]
    [JsonInclude]
    public RealmType Type { get; internal set; } = RealmType.Window;

    /// <summary>
    /// Casts this <see cref="RealmInfo"/> to a type-specific realm info, throwing if it is not of that
    /// type. Use <see cref="TryAs{T}"/> to test without throwing.
    /// </summary>
    /// <typeparam name="T">The specific type of RealmInfo to return.</typeparam>
    /// <returns>This instance cast to the specified correct type.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if this RealmInfo is not the specified type.</exception>
    public T As<T>()
        where T : RealmInfo
    {
        if (this is T castValue)
        {
            return castValue;
        }

        throw new WebDriverBiDiException($"This RealmInfo cannot be cast to {typeof(T)}");
    }

    /// <summary>
    /// Attempts to cast this <see cref="RealmInfo"/> to a type-specific realm info, returning
    /// <see langword="false"/> rather than throwing when it is not of that type.
    /// </summary>
    /// <typeparam name="T">The specific type of RealmInfo to return.</typeparam>
    /// <param name="result">When this method returns, contains the cast value or null if the cast failed.</param>
    /// <returns><see langword="true"/> if the cast was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryAs<T>([NotNullWhen(true)] out T? result)
        where T : RealmInfo
    {
        if (this is T converted)
        {
            result = converted;
            return true;
        }

        result = null;
        return false;
    }
}
