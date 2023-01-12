// <copyright file="RealmType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

/// <summary>
/// The type of realm.
/// </summary>
public enum RealmType
{
    /// <summary>
    /// A window realm.
    /// </summary>
    Window,

    /// <summary>
    /// A dedicated worker realm.
    /// </summary>
    DedicatedWorker,

    /// <summary>
    /// A shared worker realm.
    /// </summary>
    SharedWorker,

    /// <summary>
    /// A service worker realm.
    /// </summary>
    ServiceWorker,

    /// <summary>
    /// A worker realm.
    /// </summary>
    Worker,

    /// <summary>
    /// A paint worklet realm.
    /// </summary>
    PaintWorklet,

    /// <summary>
    /// An audio worklet realm.
    /// </summary>
    AudioWorklet,

    /// <summary>
    /// A worklet realm.
    /// </summary>
    Worklet,
}