// <copyright file="SetViewportCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.create command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class SetViewportCommandParameters : CommandParameters<EmptyResult>
{
    private string browsingContextId;
    private Viewport? viewport;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetViewportCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to capture the screenshot.</param>
    public SetViewportCommandParameters(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "browsingContext.setViewport";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to set the viewport.
    /// </summary>
    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the viewport dimensions to set. A null value sets the viewport to the default dimensions.
    /// </summary>
    [JsonProperty("viewport", NullValueHandling = NullValueHandling.Include)]
    public Viewport? Viewport { get => this.viewport; set => this.viewport = value; }
}
