// <copyright file="PerformActionsCommandParameters.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Input;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the input.performActions command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PerformActionsCommandParameters : CommandParameters<EmptyResult>
{
    private readonly List<SourceActions> actions = new();
    private string browsingContextId;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformActionsCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context in which to perform actions.</param>
    public PerformActionsCommandParameters(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "input.performActions";

    /// <summary>
    /// Gets or sets the browsing context ID on which to perform actions.
    /// </summary>
    [JsonProperty("context")]
    public string Context { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets the list of actions to perform.
    /// </summary>
    [JsonProperty("actions")]
    public List<SourceActions> Actions => this.actions;
}