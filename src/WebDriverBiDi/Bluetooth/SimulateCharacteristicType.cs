// <copyright file="SimulateCharacteristicType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the simulation of characteristics.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<SimulateCharacteristicType>))]
public enum SimulateCharacteristicType
{
    /// <summary>
    /// Simulate adding a characteristic.
    /// </summary>
    Add,

    /// <summary>
    /// Simulate removing a characteristic.
    /// </summary>
    Remove,
}
