// <copyright file="EnvironmentBlendingMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "environment-blending" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<EnvironmentBlendingMediaFeatureValue>))]
[StringEnumNullSentinelValue<EnvironmentBlendingMediaFeatureValue>(Reset)]
public enum EnvironmentBlendingMediaFeatureValue
{
    /// <summary>
    /// The "opaque" value for the "environment-blending" CSS media feature.
    /// </summary>
    Opaque,

    /// <summary>
    /// The "Additive" value for the "environment-blending" CSS media feature.
    /// </summary>
    Additive,

    /// <summary>
    /// The "Subtractive" value for the "environment-blending" CSS media feature.
    /// </summary>
    Subtractive,

    /// <summary>
    /// A value to reset the emulation of the "environment-blending" CSS media feature.
    /// </summary>
    Reset,
}
