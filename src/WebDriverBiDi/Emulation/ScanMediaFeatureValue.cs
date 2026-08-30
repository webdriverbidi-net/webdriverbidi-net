// <copyright file="ScanMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "scan" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ScanMediaFeatureValue>))]
[StringEnumNullSentinelValue<ScanMediaFeatureValue>(Reset)]
public enum ScanMediaFeatureValue
{
    /// <summary>
    /// The "interlace" value for the "scan" CSS media feature.
    /// </summary>
    Interlace,

    /// <summary>
    /// The "progressive" value for the "scan" CSS media feature.
    /// </summary>
    Progressive,

    /// <summary>
    /// A value to reset the emulation of the "scan" CSS media feature.
    /// </summary>
    Reset,
}
