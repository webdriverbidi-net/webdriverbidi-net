// <copyright file="HandleRequestDevicePromptAcceptCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.handleRequestDevicePrompt command to accept the prompt.
/// </summary>
public class HandleRequestDevicePromptAcceptCommandParameters : HandleRequestDevicePromptCommandParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HandleRequestDevicePromptAcceptCommandParameters"/> class to accept the device prompt.
    /// </summary>
    /// <param name="browsingContextId">The browsing context ID for which to accept the prompt.</param>
    /// <param name="promptId">The ID of the prompt to accept.</param>
    /// <param name="deviceId">The ID of the device for which to accept the prompt.</param>
    public HandleRequestDevicePromptAcceptCommandParameters(string browsingContextId, string promptId, string deviceId)
        : base(browsingContextId, promptId, true, deviceId)
    {
    }

    /// <summary>
    /// Gets or sets the ID of the device for which to accept the prompt.
    /// </summary>
    [JsonIgnore]
    public string DeviceId { get => this.SerializableDeviceId!; set => this.SerializableDeviceId = value; }
}
