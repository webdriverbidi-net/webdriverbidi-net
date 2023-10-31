// <copyright file="CloseCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.close command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CloseCommandParameters : CommandParameters<EmptyResult>
{
    private string browsingContextId;
    private bool? promptUnload;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloseCommandParameters" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context to close.</param>
    public CloseCommandParameters(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "browsingContext.close";

    /// <summary>
    /// Gets or sets the ID of the browsing context to close.
    /// </summary>
    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to prompt for unloading the page when closing the browsing context.
    /// </summary>
    [JsonProperty("promptUnload", NullValueHandling = NullValueHandling.Ignore)]
    public bool? PromptUnload { get => this.promptUnload; set => this.promptUnload = value; }
}
