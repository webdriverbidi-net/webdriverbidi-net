// <copyright file="SimulateCharacteristicResponseType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the simulation of characteristic responses.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<SimulateCharacteristicResponseType>))]
public enum SimulateCharacteristicResponseType
{
    /// <summary>
    /// Simulate a response for a read characteristic.
    /// </summary>
    Read,

    /// <summary>
    /// Simulate a response for a write characteristic.
    /// </summary>
    Write,

    /// <summary>
    /// Simulate a response for a subscribe characteristic.
    /// </summary>
    [JsonEnumValue("subscribe-to-notifications")]
    SubscribeToNotifications,

    /// <summary>
    /// Simulate a response for an unsubscribe characteristic.
    /// </summary>
    [JsonEnumValue("unsubscribe-from-notifications")]
    UnsubscribeFromNotifications,
}
