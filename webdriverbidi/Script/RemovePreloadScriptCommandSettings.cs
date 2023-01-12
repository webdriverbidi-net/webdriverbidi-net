// <copyright file="RemovePreloadScriptCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the script.removePreloadScript command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class RemovePreloadScriptCommandSettings : CommandSettings
{
    private string preloadScriptId;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePreloadScriptCommandSettings"/> class.
    /// </summary>
    /// <param name="preloadScriptId">The ID of the preload script to remove.</param>
    public RemovePreloadScriptCommandSettings(string preloadScriptId)
    {
        this.preloadScriptId = preloadScriptId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "script.removePreloadScript";

    /// <summary>
    /// Gets the type of the result of the command.
    /// </summary>
    public override Type ResultType => typeof(EmptyResult);

    /// <summary>
    /// Gets or sets the ID of the preload script to remove.
    /// </summary>
    [JsonProperty("script")]
    public string PreloadScriptId { get => this.preloadScriptId; set => this.preloadScriptId = value; }
}