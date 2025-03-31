// <copyright file="GetClientWindowsCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Result for getting the current client windows for the browser.createUserContext command.
/// </summary>
public record GetClientWindowsCommandResult : CommandResult
{
    private List<ClientWindowInfo> clientWindows = new();

    /// <summary>
    /// Gets a read-only list of information about all of the client windows open for the current browser.
    /// </summary>
    [JsonIgnore]
    public IList<ClientWindowInfo> ClientWindows => this.clientWindows.AsReadOnly();

    /// <summary>
    /// Gets or sets the information about the current browser's client windows for serialization purposes.
    /// </summary>
    [JsonPropertyName("clientWindows")]
    [JsonRequired]
    [JsonInclude]
    internal List<ClientWindowInfo> SerializableClientWindows { get => this.clientWindows; set => this.clientWindows = value; }
}
