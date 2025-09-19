// <copyright file="PreloadingStatus.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Speculation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for status of preloaded resources..
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<PreloadingStatus>))]
public enum PreloadingStatus
{
    /// <summary>
    /// The prefetch process is beginning to start a referrer-initiated navigational prefetch.
    /// </summary>
    Pending,

    /// <summary>
    /// The prefetch process is complete.
    /// </summary>
    Ready,

    /// <summary>
    /// The prefetch process activated, such as from the user agent’s navigation.
    /// </summary>
    Success,

    /// <summary>
    /// The prefetch process has failed, such as when the prefetch record expires.
    /// </summary>
    Failure,
}
