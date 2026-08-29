// <copyright file="ForcedColorsModeTheme.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the emulation of forced color themes.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ForcedColorsModeTheme>))]
public enum ForcedColorsModeTheme
{
    /// <summary>
    /// Emulate forced colors for "light mode" theme.
    /// </summary>
    Light,

    /// <summary>
    /// Emulate forced colors for "dark mode" theme.
    /// </summary>
    Dark,
}
