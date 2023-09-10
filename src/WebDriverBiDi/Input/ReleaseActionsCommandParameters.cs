// <copyright file="ReleaseActionsCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the input.releaseActions command.
/// </summary>
public class ReleaseActionsCommandParameters : CommandParameters<EmptyResult>
{
    private string browsingContextId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReleaseActionsCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to release the pending actions.</param>
    public ReleaseActionsCommandParameters(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "input.releaseActions";

    /// <summary>
    /// Gets or sets the browsing context ID for which to release pending actions.
    /// </summary>
    [JsonPropertyName("context")]
    public string Context { get => this.browsingContextId; set => this.browsingContextId = value; }
}