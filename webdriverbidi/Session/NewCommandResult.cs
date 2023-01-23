// <copyright file="NewCommandResult.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Session;

using Newtonsoft.Json;

/// <summary>
/// Result for creating a new sesstion using the session.new command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class NewCommandResult : ResponseData
{
    private string sessionId = string.Empty;

    private CapabilitiesResult capabilitiesResult = new();

    [JsonConstructor]
    private NewCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the session.
    /// </summary>
    [JsonProperty("sessionId")]
    [JsonRequired]
    public string SessionId { get => this.sessionId; internal set => this.sessionId = value; }

    /// <summary>
    /// Gets the actual capabilities used in this session.
    /// </summary>
    [JsonProperty("capabilities")]
    [JsonRequired]
    public CapabilitiesResult Capabilities { get => this.capabilitiesResult; internal set => this.capabilitiesResult = value; }
}