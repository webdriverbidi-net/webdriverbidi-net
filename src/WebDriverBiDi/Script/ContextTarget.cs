// <copyright file="ContextTarget.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object representing a script target that is a browsing context.
/// </summary>
public class ContextTarget : Target
{
    private string browsingContextId;
    private string? sandbox;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextTarget"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context of the script target.</param>
    [JsonConstructor]
    public ContextTarget(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the ID of the browsing context used as a script target.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get => this.browsingContextId; private set => this.browsingContextId = value; }

    /// <summary>
    /// Gets the name of the sandbox.
    /// </summary>
    [JsonPropertyName("sandbox")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? Sandbox { get => this.sandbox; set => this.sandbox = value; }
}
