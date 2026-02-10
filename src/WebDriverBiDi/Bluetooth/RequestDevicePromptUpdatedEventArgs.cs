// <copyright file="RequestDevicePromptUpdatedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for events raised when a Bluetooth device prompt is requested.
/// </summary>
public record RequestDevicePromptUpdatedEventArgs : WebDriverBiDiEventArgs
{
    [JsonConstructor]
    private RequestDevicePromptUpdatedEventArgs()
    {
    }

    /// <summary>
    /// Gets the ID of the browsing context prompting for the update.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the ID of the prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    [JsonRequired]
    [JsonInclude]
    public string Prompt { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the read-only list of devices being requested in the prompt.
    /// </summary>
    public IList<RequestDeviceInfo> Devices => this.SerializableDevices.AsReadOnly();

    /// <summary>
    /// Gets or sets the read-only list of devices being requested in the prompt.
    /// </summary>
    [JsonPropertyName("devices")]
    [JsonRequired]
    [JsonInclude]
    internal List<RequestDeviceInfo> SerializableDevices { get; set; } = [];
}
