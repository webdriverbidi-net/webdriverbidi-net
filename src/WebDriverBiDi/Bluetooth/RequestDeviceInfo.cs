// <copyright file="RequestDeviceInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides information about a Bluetooth device during a request.
/// </summary>
public class RequestDeviceInfo
{
    private string id = string.Empty;
    private string? name;

    [JsonConstructor]
    private RequestDeviceInfo()
    {
    }

    /// <summary>
    /// Gets the ID of the Bluetooth device.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonRequired]
    [JsonInclude]
    public string DeviceId { get => this.id; private set => this.id = value; }

    /// <summary>
    /// Gets the name of the Bluetooth device, if specified.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    [JsonInclude]
    public string? DeviceName { get => this.name; private set => this.name = value; }
}
