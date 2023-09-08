// <copyright file="UserPromptClosedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Object containing event data for the event raised when a user prompt is closed.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class UserPromptClosedEventArgs : WebDriverBiDiEventArgs
{
    private string browsingContextId;
    private bool isAccepted;
    private string? userText;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPromptClosedEventArgs" /> class.
    /// </summary>
    /// <param name="browsingContextId">The browsing context for which the user prompt was closed.</param>
    /// <param name="isAccepted">A value of true if the user prompt was accepted; false if it was canceled.</param>
    [JsonConstructor]
    public UserPromptClosedEventArgs(string browsingContextId, bool isAccepted)
    {
        this.browsingContextId = browsingContextId;
        this.isAccepted = isAccepted;
    }

    /// <summary>
    /// Gets the ID of the browsing context for which the user prompt was closed.
    /// </summary>
    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; internal set => this.browsingContextId = value; }

    /// <summary>
    /// Gets a value indicating whether the user prompt was accepted (true), or if it was canceled (false).
    /// </summary>
    [JsonProperty("accepted")]
    [JsonRequired]
    public bool IsAccepted { get => this.isAccepted; internal set => this.isAccepted = value; }

    /// <summary>
    /// Gets the text of the user prompt.
    /// </summary>
    [JsonProperty("userText", NullValueHandling = NullValueHandling.Ignore)]
    public string? UserText { get => this.userText; internal set => this.userText = value; }
}
