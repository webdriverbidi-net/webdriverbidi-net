// <copyright file="AddPreloadScriptCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

/// <summary>
/// Result for adding a preload script using the script.addPreloadScript command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class AddPreloadScriptCommandResult : CommandResult
{
    private string preloadScriptId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPreloadScriptCommandResult"/> class.
    /// </summary>
    [JsonConstructor]
    internal AddPreloadScriptCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the preload script.
    /// </summary>
    [JsonProperty("script")]
    public string PreloadScriptId { get => this.preloadScriptId; internal set => this.preloadScriptId = value; }
}