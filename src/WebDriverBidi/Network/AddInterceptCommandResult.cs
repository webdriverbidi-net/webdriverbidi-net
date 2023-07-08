// <copyright file="AddInterceptCommandResult.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// Result for adding an intercept for network traffic using the network.addIntercept command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class AddInterceptCommandResult : CommandResult
{
    private string interceptId = string.Empty;

    [JsonConstructor]
    private AddInterceptCommandResult()
    {
    }

    /// <summary>
    /// Gets the screenshot image data as a base64-encoded string.
    /// </summary>
    [JsonProperty("intercept")]
    [JsonRequired]
    public string InterceptId { get => this.interceptId; internal set => this.interceptId = value; }
}
