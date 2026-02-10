// <copyright file="GattConnectionAttemptedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for events raised when a Bluetooth GATT connection is attempted.
/// </summary>
public record GattConnectionAttemptedEventArgs : WebDriverBiDiEventArgs
{
    [JsonConstructor]
    private GattConnectionAttemptedEventArgs()
    {
    }

    /// <summary>
    /// Gets the ID of the browsing context attempting the connection.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the address of the device attempting the connection.
    /// </summary>
    [JsonPropertyName("address")]
    [JsonRequired]
    [JsonInclude]
    public string Address { get; internal set; } = string.Empty;
}
