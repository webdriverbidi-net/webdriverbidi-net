// <copyright file="ForcedColorsMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "forced-colors" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ForcedColorsMediaFeatureValue>))]
[StringEnumNullSentinelValue<ForcedColorsMediaFeatureValue>(Reset)]
public enum ForcedColorsMediaFeatureValue
{
    /// <summary>
    /// The "none" value for the "forced-colors" CSS media feature.
    /// </summary>
    None,

    /// <summary>
    /// The "active" value for the "forced-colors" CSS media feature.
    /// </summary>
    Active,

    /// <summary>
    /// A value to reset the emulation of the "forced-colors" CSS media feature.
    /// </summary>
    Reset,
}
