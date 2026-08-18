// <copyright file="PrefersContrastMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "prefers-contrast" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<PrefersContrastMediaFeatureValue>))]
[StringEnumNullSentinelValue<PrefersContrastMediaFeatureValue>(Reset)]
public enum PrefersContrastMediaFeatureValue
{
    /// <summary>
    /// The "no-preference" value for the "prefers-contrast" CSS media feature.
    /// </summary>
    [StringEnumValue("no-preference")]
    NoPreference,

    /// <summary>
    /// The "more" value for the "prefers-contrast" CSS media feature.
    /// </summary>
    More,

    /// <summary>
    /// The "less" value for the "prefers-contrast" CSS media feature.
    /// </summary>
    Less,

    /// <summary>
    /// The "custom" value for the "prefers-contrast" CSS media feature.
    /// </summary>
    Custom,

    /// <summary>
    /// A value to reset the emulation of the "prefers-contrast" CSS media feature.
    /// </summary>
    Reset,
}
