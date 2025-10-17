// <copyright file="CharacteristicEventGeneratedType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the type of characteristic event generated.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<CharacteristicEventGeneratedType>))]
public enum CharacteristicEventGeneratedType
{
    /// <summary>
    /// The characteristic event is a read event.
    /// </summary>
    Read,

    /// <summary>
    /// The characteristic event is a write event with a response.
    /// </summary>
    [JsonEnumValue("write-with-response")]
    WriteWithResponse,

    /// <summary>
    /// The characteristic event is a write event without a response.
    /// </summary>
    [JsonEnumValue("write-without-response")]
    WriteWithoutResponse,

    /// <summary>
    /// The characteristic event is a subscription to notifications.
    /// </summary>
    [JsonEnumValue("subscribe-to-notifications")]
    SubscribeToNotifications,

    /// <summary>
    /// The characteristic event is an unsubscription from notifications.
    /// </summary>
    [JsonEnumValue("unsubscribe-from-notifications")]
    UnsubscribeFromNotifications,
}
