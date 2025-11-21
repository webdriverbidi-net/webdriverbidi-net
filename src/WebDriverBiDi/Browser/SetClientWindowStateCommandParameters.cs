// <copyright file="SetClientWindowStateCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browser.setClientWindowState command.
/// </summary>
public class SetClientWindowStateCommandParameters : CommandParameters<SetClientWindowStateCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetClientWindowStateCommandParameters"/> class.
    /// </summary>
    /// <param name="clientWindowId">The ID of the client window for which to set the state.</param>
    public SetClientWindowStateCommandParameters(string clientWindowId)
    {
        this.ClientWindowId = clientWindowId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browser.setClientWindowState";

    /// <summary>
    /// Gets or sets the ID of the client window for which to set the state.
    /// </summary>
    [JsonPropertyName("clientWindow")]
    [JsonInclude]
    public string ClientWindowId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the state of the client window.
    /// </summary>
    [JsonPropertyName("state")]
    [JsonInclude]
    public ClientWindowState State { get; set; } = ClientWindowState.Normal;

    /// <summary>
    /// Gets or sets the value in CSS pixels of the left edge of the client window.
    /// This parameter is ignored if the <see cref="State"/> property is set to a value other
    /// than <see cref="ClientWindowState.Normal"/>.
    /// </summary>
    [JsonPropertyName("x")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? X { get => this.State == ClientWindowState.Normal ? field : null; set; }

    /// <summary>
    /// Gets or sets the value in CSS pixels of the top edge of the client window.
    /// This parameter is ignored if the <see cref="State"/> property is set to a value other
    /// than <see cref="ClientWindowState.Normal"/>.
    /// </summary>
    [JsonPropertyName("y")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Y { get => this.State == ClientWindowState.Normal ? field : null; set; }

    /// <summary>
    /// Gets or sets the value in CSS pixels of the width of the client window.
    /// This parameter is ignored if the <see cref="State"/> property is set to a value other
    /// than <see cref="ClientWindowState.Normal"/>.
    /// </summary>
    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Width { get => this.State == ClientWindowState.Normal ? field : null; set; }

    /// <summary>
    /// Gets or sets the value in CSS pixels of the height of the client window.
    /// This parameter is ignored if the <see cref="State"/> property is set to a value other
    /// than <see cref="ClientWindowState.Normal"/>.
    /// </summary>
    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Height { get => this.State == ClientWindowState.Normal ? field : null; set; }
}
