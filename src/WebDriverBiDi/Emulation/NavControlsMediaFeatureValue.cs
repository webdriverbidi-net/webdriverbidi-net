// <copyright file="NavControlsMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "nav-controls" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<NavControlsMediaFeatureValue>))]
[StringEnumNullSentinelValue<NavControlsMediaFeatureValue>(Reset)]
public enum NavControlsMediaFeatureValue
{
    /// <summary>
    /// The "none" value for the "nav-controls" CSS media feature.
    /// </summary>
    None,

    /// <summary>
    /// The "back" value for the "nav-controls" CSS media feature.
    /// </summary>
    Back,

    /// <summary>
    /// A value to reset the emulation of the "nav-controls" CSS media feature.
    /// </summary>
    Reset,
}
