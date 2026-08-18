// <copyright file="HoverMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "hover" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<HoverMediaFeatureValue>))]
[StringEnumNullSentinelValue<HoverMediaFeatureValue>(Reset)]
public enum HoverMediaFeatureValue
{
    /// <summary>
    /// The "none" value for the "hover" CSS media feature.
    /// </summary>
    None,

    /// <summary>
    /// The "hover" value for the "hover" CSS media feature.
    /// </summary>
    Hover,

    /// <summary>
    /// A value to reset the emulation of the "hover" CSS media feature.
    /// </summary>
    Reset,
}
