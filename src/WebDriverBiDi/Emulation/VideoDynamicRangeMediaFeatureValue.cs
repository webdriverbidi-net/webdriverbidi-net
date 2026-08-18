// <copyright file="VideoDynamicRangeMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "video-dynamic-range" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<VideoDynamicRangeMediaFeatureValue>))]
[StringEnumNullSentinelValue<VideoDynamicRangeMediaFeatureValue>(Reset)]
public enum VideoDynamicRangeMediaFeatureValue
{
    /// <summary>
    /// The "standard" value for the "video-dynamic-range" CSS media feature.
    /// </summary>
    Standard,

    /// <summary>
    /// The "high" value for the "video-dynamic-range" CSS media feature.
    /// </summary>
    High,

    /// <summary>
    /// A value to reset the emulation of the "video-dynamic-range" CSS media feature.
    /// </summary>
    Reset,
}
