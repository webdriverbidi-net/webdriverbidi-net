// <copyright file="DynamicRangeMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "dynamic-range" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<DynamicRangeMediaFeatureValue>))]
[StringEnumNullSentinelValue<DynamicRangeMediaFeatureValue>(Reset)]
public enum DynamicRangeMediaFeatureValue
{
    /// <summary>
    /// The "standard" value for the "dynamic-range" CSS media feature.
    /// </summary>
    Standard,

    /// <summary>
    /// The "high" value for the "dynamic-range" CSS media feature.
    /// </summary>
    High,

    /// <summary>
    /// A value to reset the emulation of the "dynamic-range" CSS media feature.
    /// </summary>
    Reset,
}
