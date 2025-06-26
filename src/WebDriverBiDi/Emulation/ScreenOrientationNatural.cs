// <copyright file="ScreenOrientationNatural.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated values for the natural screen orientation of a display.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ScreenOrientationNatural>))]
public enum ScreenOrientationNatural
{
    /// <summary>
    /// The natural screen orientation is portrait.
    /// </summary>
    Portrait,

    /// <summary>
    /// The natural screen orientation is landscape.
    /// </summary>
    Landscape,
}
