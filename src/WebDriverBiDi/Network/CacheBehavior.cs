// <copyright file="CacheBehavior.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated value of types for a BytesValue.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<CacheBehavior>))]
public enum CacheBehavior
{
    /// <summary>
    /// The browser uses the default cache behavior.
    /// </summary>
    Default,

    /// <summary>
    /// The browser bypasses the cache.
    /// </summary>
    Bypass,
}
