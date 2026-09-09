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
    public VirtualWalletAction Action { get; set; }

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to set the virtual wallet action.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the protocol identifier the simulated credential is presented under.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is not a filter: it names the protocol of the credential the wallet returns, and its value
    /// reaches the page as the presented credential's protocol. Only <see cref="BrowsingContextId"/>
    /// scopes the behavior.
    /// </para>
    /// <para>
    /// This property and <see cref="Response"/> are required together when <see cref="Action"/> is
    /// <see cref="VirtualWalletAction.Respond"/>, and must both be omitted for every other action. The
    /// value must be one of the protocol identifiers enumerated by the Digital Credentials API. This
    /// property does not validate either rule; a conforming remote end answers with an "invalid argument"
    /// error when the command is executed.
    /// </para>
    /// </remarks>
    [JsonPropertyName("protocol")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Protocol { get; set; }

    /// <summary>
    /// Gets or sets the response object to use as the credential data.
    /// </summary>
    /// <remarks>
    /// This property and <see cref="Protocol"/> are required together when <see cref="Action"/> is
    /// <see cref="VirtualWalletAction.Respond"/>, and must both be omitted for every other action. This
    /// property does not validate that rule; a conforming remote end answers with an "invalid argument"
    /// error when the command is executed.
    /// </remarks>
    [JsonPropertyName("response")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? Response { get; set; }
}
