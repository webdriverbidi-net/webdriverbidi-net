// <copyright file="PrefersColorSchemeFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "prefers-color-scheme" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<PrefersColorSchemeFeatureValue>))]
[StringEnumNullSentinelValue<PrefersColorSchemeFeatureValue>(Reset)]
public enum PrefersColorSchemeFeatureValue
{
    /// <summary>
    /// The "light" value for the "prefers-color-scheme" CSS media feature.
    /// </summary>
    Light,

    /// <summary>
    /// The "dark" value for the "prefers-color-scheme" CSS media feature.
    /// </summary>
    Dark,

    /// <summary>
    /// A value to reset the emulation of the "prefers-color-scheme" CSS media feature.
    /// </summary>
    Reset,
}
