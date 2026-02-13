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
public record SetClientWindowStateCommandResult : CommandResult
{
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
    public string ClientWindowId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether a client window is active, usually implying it has focus in the operating system.
    /// </summary>
    [JsonPropertyName("active")]
    [JsonInclude]
    [JsonRequired]
    public bool IsActive { get; internal set; } = false;

    /// <summary>
    /// Gets a value indicating the state of the client window.
    /// </summary>
    [JsonPropertyName("state")]
    [JsonInclude]
    [JsonRequired]
    public ClientWindowState State { get; internal set; } = ClientWindowState.Normal;

    /// <summary>
    /// Gets the value in CSS pixels of the left edge of the client window.
    /// </summary>
    [JsonPropertyName("x")]
    [JsonInclude]
    [JsonRequired]
    public ulong X { get; internal set; } = 0;

    /// <summary>
    /// Gets the value in CSS pixels of the top edge of the client window.
    /// </summary>
    [JsonPropertyName("y")]
    [JsonInclude]
    [JsonRequired]
    public ulong Y { get; internal set; } = 0;

    /// <summary>
    /// Gets the value in CSS pixels of the width of the client window.
    /// </summary>
    [JsonPropertyName("width")]
    [JsonInclude]
    [JsonRequired]
    public ulong Width { get; internal set; } = 0;

    /// <summary>
    /// Gets the value in CSS pixels of the height of the client window.
    /// </summary>
    [JsonPropertyName("height")]
    [JsonInclude]
    [JsonRequired]
    public ulong Height { get; internal set; } = 0;
}
