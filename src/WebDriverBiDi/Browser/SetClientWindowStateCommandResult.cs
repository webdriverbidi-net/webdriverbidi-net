// <copyright file="SetClientWindowStateCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Result for getting the current client windows for the browser.setClientWindowState command.
/// </summary>
/// <remarks>
/// This result class is essentially a copy of <see cref="ClientWindowInfo"/>, as that is the
/// type specified by the protocol. This is structured this way as commands must return a
/// type descending from <see cref="CommandResult"/>, and C# does not permit multiple inheritance.
/// Should the structure of the info object change, this class will require updates to match.
/// </remarks>
public class SetClientWindowStateCommandResult : CommandResult
{
    private string clientWindowId = string.Empty;
    private bool isActive = false;
    private ClientWindowState state = ClientWindowState.Normal;
    private ulong x = 0;
    private ulong y = 0;
    private ulong width = 0;
    private ulong height = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetClientWindowStateCommandResult"/> class.
    /// </summary>
    private SetClientWindowStateCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the client window.
    /// </summary>
    [JsonPropertyName("clientWindow")]
    [JsonInclude]
    [JsonRequired]
    public string ClientWindowId { get => this.clientWindowId; private set => this.clientWindowId = value; }

    /// <summary>
    /// Gets a value indicating whether a client window is active, usually implying it has focus in the operating system.
    /// </summary>
    [JsonPropertyName("active")]
    [JsonInclude]
    [JsonRequired]
    public bool IsActive { get => this.isActive; private set => this.isActive = value; }

    /// <summary>
    /// Gets a value indicating the state of the client window.
    /// </summary>
    [JsonPropertyName("state")]
    [JsonInclude]
    [JsonRequired]
    public ClientWindowState State { get => this.state;  private set => this.state = value; }

    /// <summary>
    /// Gets the value in CSS pixels of the left edge of the client window.
    /// </summary>
    [JsonPropertyName("x")]
    [JsonInclude]
    [JsonRequired]
    public ulong X { get => this.x; private set => this.x = value; }

    /// <summary>
    /// Gets the value in CSS pixels of the top edge of the client window.
    /// </summary>
    [JsonPropertyName("y")]
    [JsonInclude]
    [JsonRequired]
    public ulong Y { get => this.y; private set => this.y = value; }

    /// <summary>
    /// Gets the value in CSS pixels of the width of the client window.
    /// </summary>
    [JsonPropertyName("width")]
    [JsonInclude]
    [JsonRequired]
    public ulong Width { get => this.width; private set => this.width = value; }

    /// <summary>
    /// Gets the value in CSS pixels of the height of the client window.
    /// </summary>
    [JsonPropertyName("height")]
    [JsonInclude]
    [JsonRequired]
    public ulong Height { get => this.height; private set => this.height = value; }
}
