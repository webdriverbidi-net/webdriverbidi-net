// <copyright file="AdapterState.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the creation of new browsing contexts.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<AdapterState>))]
public enum AdapterState
{
    /// <summary>
    /// The Bluetooth adapter is absent.
    /// </summary>
    Absent,

    /// <summary>
    /// The Bluetooth adapter is present, but powered off.
    /// </summary>
    [JsonEnumValue("powered-off")]
    PoweredOff,

    /// <summary>
    /// The Bluetooth adapter is present, and powered on.
    /// </summary>
    [JsonEnumValue("powered-on")]
    PoweredOn,
}
