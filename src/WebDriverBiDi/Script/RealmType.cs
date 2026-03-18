// <copyright file="RealmType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The type of realm.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<RealmType>))]
public enum RealmType
{
    /// <summary>
    /// A window realm.
    /// </summary>
    [StringEnumValue("window")]
    Window,

    /// <summary>
    /// A dedicated worker realm.
    /// </summary>
    [StringEnumValue("dedicated-worker")]
    DedicatedWorker,

    /// <summary>
    /// A shared worker realm.
    /// </summary>
    [StringEnumValue("shared-worker")]
    SharedWorker,

    /// <summary>
    /// A service worker realm.
    /// </summary>
    [StringEnumValue("service-worker")]
    ServiceWorker,

    /// <summary>
    /// A worker realm.
    /// </summary>
    [StringEnumValue("worker")]
    Worker,

    /// <summary>
    /// A paint worklet realm.
    /// </summary>
    [StringEnumValue("paint-worklet")]
    PaintWorklet,

    /// <summary>
    /// An audio worklet realm.
    /// </summary>
    [StringEnumValue("audio-worklet")]
    AudioWorklet,

    /// <summary>
    /// A worklet realm.
    /// </summary>
    [StringEnumValue("worklet")]
    Worklet,
}
