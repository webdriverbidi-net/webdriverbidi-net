// <copyright file="RequestDevicePromptUpdatedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;
using WebDriverBiDi.Storage;

/// <summary>
/// Object containing event data for events raised when a Bluetooth device prompt is requested.
/// </summary>
public record RequestDevicePromptUpdatedEventArgs : WebDriverBiDiEventArgs
{
    private string browsingContextId = string.Empty;
    private string prompt = string.Empty;
    private List<RequestDeviceInfo> devices = new();

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
    public string BrowsingContextId { get => this.browsingContextId;  private set => this.browsingContextId = value; }

    /// <summary>
    /// Gets the ID of the prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    [JsonRequired]
    [JsonInclude]
    public string Prompt { get => this.prompt; private set => this.prompt = value; }

    /// <summary>
    /// Gets the read-only list of devices being requested in the prompt.
    /// </summary>
    public IList<RequestDeviceInfo> Devices => this.devices.AsReadOnly();

    /// <summary>
    /// Gets or sets the read-only list of devices being requested in the prompt.
    /// </summary>
    [JsonPropertyName("devices")]
    [JsonRequired]
    [JsonInclude]
    internal List<RequestDeviceInfo> SerializableDevices { get => this.devices; set => this.devices = value; }
}
