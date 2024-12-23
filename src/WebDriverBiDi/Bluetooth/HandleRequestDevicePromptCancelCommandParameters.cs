// <copyright file="HandleRequestDevicePromptCancelCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.handleRequestDevicePrompt command to cancel the prompt.
/// </summary>
public class HandleRequestDevicePromptCancelCommandParameters : HandleRequestDevicePromptCommandParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HandleRequestDevicePromptCancelCommandParameters"/> class to cancel the device prompt.
    /// </summary>
    /// <param name="browsingContextId">The browsing context ID for which to accept the prompt.</param>
    /// <param name="promptId">The ID of the prompt to accept.</param>
    public HandleRequestDevicePromptCancelCommandParameters(string browsingContextId, string promptId)
        : base(browsingContextId, promptId, false)
    {
    }
}
