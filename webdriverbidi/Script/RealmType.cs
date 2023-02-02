// <copyright file="RealmType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// The type of realm.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<RealmType>))]
public enum RealmType
{
    /// <summary>
    /// A window realm.
    /// </summary>
    [JsonEnumValue("window")]
    Window,

    /// <summary>
    /// A dedicated worker realm.
    /// </summary>
    [JsonEnumValue("dedicated-worker")]
    DedicatedWorker,

    /// <summary>
    /// A shared worker realm.
    /// </summary>
    [JsonEnumValue("shared-worker")]
    SharedWorker,

    /// <summary>
    /// A service worker realm.
    /// </summary>
    [JsonEnumValue("service-worker")]
    ServiceWorker,

    /// <summary>
    /// A worker realm.
    /// </summary>
    [JsonEnumValue("worker")]
    Worker,

    /// <summary>
    /// A paint worklet realm.
    /// </summary>
    [JsonEnumValue("paint-worklet")]
    PaintWorklet,

    /// <summary>
    /// An audio worklet realm.
    /// </summary>
    [JsonEnumValue("audio-worklet")]
    AudioWorklet,

    /// <summary>
    /// A worklet realm.
    /// </summary>
    [JsonEnumValue("worklet")]
    Worklet,
}