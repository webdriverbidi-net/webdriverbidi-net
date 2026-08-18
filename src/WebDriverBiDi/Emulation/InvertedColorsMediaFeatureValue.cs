// <copyright file="InvertedColorsMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "inverted-colors" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<InvertedColorsMediaFeatureValue>))]
[StringEnumNullSentinelValue<InvertedColorsMediaFeatureValue>(Reset)]
public enum InvertedColorsMediaFeatureValue
{
    /// <summary>
    /// The "none" value for the "inverted-colors" CSS media feature.
    /// </summary>
    None,

    /// <summary>
    /// The "inverted" value for the "inverted-colors" CSS media feature.
    /// </summary>
    Inverted,

    /// <summary>
    /// A value to reset the emulation of the "inverted-colors" CSS media feature.
    /// </summary>
    Reset,
}
