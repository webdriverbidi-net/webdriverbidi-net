// <copyright file="PrefersReducedMotionMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "prefers-reduced-motion" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<PrefersReducedMotionMediaFeatureValue>))]
[StringEnumNullSentinelValue<PrefersReducedMotionMediaFeatureValue>(Reset)]
public enum PrefersReducedMotionMediaFeatureValue
{
    /// <summary>
    /// The "no-preference" value for the "prefers-reduced-motion" CSS media feature.
    /// </summary>
    [StringEnumValue("no-preference")]
    NoPreference,

    /// <summary>
    /// The "reduce" value for the "prefers-reduced-motion" CSS media feature.
    /// </summary>
    Reduce,

    /// <summary>
    /// A value to reset the emulation of the "prefers-reduced-motion" CSS media feature.
    /// </summary>
    Reset,
}
