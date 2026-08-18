// <copyright file="ColorGamutMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "color-gamut" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ColorGamutMediaFeatureValue>))]
[StringEnumNullSentinelValue<ColorGamutMediaFeatureValue>(Reset)]
public enum ColorGamutMediaFeatureValue
{
    /// <summary>
    /// The "srgb" value for the "color-gamut" CSS media feature.
    /// </summary>
    Srgb,

    /// <summary>
    /// The "p3" value for the "color-gamut" CSS media feature.
    /// </summary>
    P3,

    /// <summary>
    /// The "rec2020" value for the "color-gamut" CSS media feature.
    /// </summary>
    Rec2020,

    /// <summary>
    /// A value to reset the emulation of the "color-gamut" CSS media feature.
    /// </summary>
    Reset,
}
