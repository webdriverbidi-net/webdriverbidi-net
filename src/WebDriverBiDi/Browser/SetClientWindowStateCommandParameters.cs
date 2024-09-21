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
    private string clientWindowId;
    private ClientWindowState state = ClientWindowState.Normal;
    private ulong? x;
    private ulong? y;
    private ulong? width;
    private ulong? height;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetClientWindowStateCommandParameters"/> class.
    /// </summary>
    /// <param name="clientWindowId">The ID of the client window for which to set the state.</param>
    public SetClientWindowStateCommandParameters(string clientWindowId)
    {
        this.clientWindowId = clientWindowId;
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
    public string ClientWindowId { get => this.clientWindowId; set => this.clientWindowId = value; }

    /// <summary>
    /// Gets or sets a value indicating the state of the client window.
    /// </summary>
    [JsonPropertyName("state")]
    [JsonInclude]
    public ClientWindowState State { get => this.state;  set => this.state = value; }

    /// <summary>
    /// Gets or sets the value in CSS pixels of the left edge of the client window.
    /// This parameter is ignored if the <see cref="State"/> property is set to a value other
    /// than <see cref="ClientWindowState.Normal"/>.
    /// </summary>
    [JsonPropertyName("x")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? X { get => this.state == ClientWindowState.Normal ? this.x : null; set => this.x = value; }

    /// <summary>
    /// Gets or sets the value in CSS pixels of the top edge of the client window.
    /// This parameter is ignored if the <see cref="State"/> property is set to a value other
    /// than <see cref="ClientWindowState.Normal"/>.
    /// </summary>
    [JsonPropertyName("y")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Y { get => this.state == ClientWindowState.Normal ? this.y : null; set => this.y = value; }

    /// <summary>
    /// Gets or sets the value in CSS pixels of the width of the client window.
    /// This parameter is ignored if the <see cref="State"/> property is set to a value other
    /// than <see cref="ClientWindowState.Normal"/>.
    /// </summary>
    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Width { get => this.state == ClientWindowState.Normal ? this.width : null; set => this.width = value; }

    /// <summary>
    /// Gets or sets the value in CSS pixels of the height of the client window.
    /// This parameter is ignored if the <see cref="State"/> property is set to a value other
    /// than <see cref="ClientWindowState.Normal"/>.
    /// </summary>
    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Height { get => this.state == ClientWindowState.Normal ? this.height : null; set => this.height = value; }
}
