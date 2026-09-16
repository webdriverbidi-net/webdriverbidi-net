// <copyright file="TextLayoutMode.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of text layout mode.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<TextLayoutMode>))]
public enum TextLayoutMode
{
    /// <summary>
    /// Emulate the text layout mode for mobile devices.
    /// </summary>
    Mobile,
}
