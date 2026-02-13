// <copyright file="RequestDeviceInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides information about a Bluetooth device during a request.
/// </summary>
public record RequestDeviceInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestDeviceInfo"/> class.
    /// </summary>
    [JsonConstructor]
    internal RequestDeviceInfo()
    {
    }

    /// <summary>
    /// Gets the ID of the Bluetooth device.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonRequired]
    [JsonInclude]
    public string DeviceId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the name of the Bluetooth device, if specified.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    [JsonInclude]
    public string? DeviceName { get; internal set; }
}
