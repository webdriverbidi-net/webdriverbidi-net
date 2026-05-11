// <copyright file="SetVirtualWalletBehaviorCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.DigitalCredentials;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the digitalCredentials.setVirtualWalletBehavior command.
/// </summary>
public class SetVirtualWalletBehaviorCommandParameters : CommandParameters<SetVirtualWalletBehaviorCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetVirtualWalletBehaviorCommandParameters"/> class.
    /// </summary>
    /// <param name="action">The <see cref="VirtualWalletAction"/> to set.</param>
    public SetVirtualWalletBehaviorCommandParameters(VirtualWalletAction action)
    {
        this.Action = action;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "digitalCredentials.setVirtualWalletBehavior";

    /// <summary>
    /// Gets or sets the action to take with the virtual wallet.
    /// </summary>
    [JsonPropertyName("action")]
    [JsonRequired]
    [JsonInclude]
    public VirtualWalletAction Action { get; set; }

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to set the virtual wallet action.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Context { get; set; }

    /// <summary>
    /// Gets or sets the protocol identifier to use for the virtual wallet behavior.
    /// </summary>
    [JsonPropertyName("protocol")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Protocol { get; set; }

    /// <summary>
    /// Gets or sets the response object to use as the credential data.
    /// </summary>
    [JsonPropertyName("response")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? Response { get; set; }
}
