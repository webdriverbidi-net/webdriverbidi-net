// <copyright file="ActivateCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.activate command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ActivateCommandParameters : CommandParameters<EmptyResult>
{
    private string browsingContextId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivateCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context to activate.</param>
    public ActivateCommandParameters(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "browsingContext.activate";

    /// <summary>
    /// Gets or sets the ID of the browsing context to activate.
    /// </summary>
    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }
}