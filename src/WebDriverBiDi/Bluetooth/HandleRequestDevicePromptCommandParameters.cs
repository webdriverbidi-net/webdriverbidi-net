// <copyright file="HandleRequestDevicePromptCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.handleRequestDevicePrompt command.
/// </summary>
public class HandleRequestDevicePromptCommandParameters : CommandParameters<EmptyResult>
{
    private readonly bool accept;
    private string browsingContextId;
    private string promptId;
    private string? deviceId;

    /// <summary>
    /// Initializes a new instance of the <see cref="HandleRequestDevicePromptCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The browsing context ID for which to handle the prompt.</param>
    /// <param name="promptId">The ID of the prompt to handle.</param>
    /// <param name="accept">A value indicating whether to accept the prompt.</param>
    /// <param name="deviceId">The ID of the device for which to accept the prompt, if the prompt is being accepted.</param>
    protected HandleRequestDevicePromptCommandParameters(string browsingContextId, string promptId, bool accept, string? deviceId = null)
    {
        this.browsingContextId = browsingContextId;
        this.promptId = promptId;
        this.accept = accept;
        this.deviceId = deviceId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "bluetooth.handleRequestDevicePrompt";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to handle the prompt.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonInclude]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the ID of the prompt to handle.
    /// </summary>
    [JsonPropertyName("prompt")]
    [JsonInclude]
    public string PromptId { get => this.promptId; set => this.promptId = value; }

    /// <summary>
    /// Gets a value indicating whether to accept the prompt.
    /// </summary>
    [JsonPropertyName("accept")]
    [JsonInclude]
    public bool Accept => this.accept;

    /// <summary>
    /// Gets or sets the ID of the device for serialization purposes.
    /// </summary>
    [JsonPropertyName("device")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal string? SerializableDeviceId { get => this.deviceId; set => this.deviceId = value; }
}
