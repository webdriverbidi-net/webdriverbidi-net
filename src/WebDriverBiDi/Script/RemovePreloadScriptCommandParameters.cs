// <copyright file="RemovePreloadScriptCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the script.removePreloadScript command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class RemovePreloadScriptCommandParameters : CommandParameters<EmptyResult>
{
    private string preloadScriptId;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePreloadScriptCommandParameters"/> class.
    /// </summary>
    /// <param name="preloadScriptId">The ID of the preload script to remove.</param>
    public RemovePreloadScriptCommandParameters(string preloadScriptId)
    {
        this.preloadScriptId = preloadScriptId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "script.removePreloadScript";

    /// <summary>
    /// Gets or sets the ID of the preload script to remove.
    /// </summary>
    [JsonProperty("script")]
    public string PreloadScriptId { get => this.preloadScriptId; set => this.preloadScriptId = value; }
}
