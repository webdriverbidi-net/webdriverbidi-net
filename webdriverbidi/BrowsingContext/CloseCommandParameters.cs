// <copyright file="CloseCommandParameters.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.close command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CloseCommandParameters : CommandParameters<EmptyResult>
{
    private string browsingContextId;

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
}