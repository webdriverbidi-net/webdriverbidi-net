// <copyright file="DisownCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using System;
using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the script.disown command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class DisownCommandSettings : CommandData<EmptyResult>
{
    private List<string> handles = new();
    private ScriptTarget target;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisownCommandSettings"/> class.
    /// </summary>
    /// <param name="target">The script target containing handles to disown.</param>
    /// <param name="handleValues">The handles to disown.</param>
    public DisownCommandSettings(ScriptTarget target, params string[] handleValues)
    {
        this.target = target;
        this.handles.AddRange(handleValues);
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "script.disown";

    /// <summary>
    /// Gets or sets the target for which to disown handles.
    /// </summary>
    [JsonProperty("target")]
    public ScriptTarget Target { get => this.target; set => this.target = value; }

    /// <summary>
    /// Gets or sets the list of handles to disown.
    /// </summary>
    [JsonProperty("handles")]
    public List<string> Handles { get => this.handles; set => this.handles = value; }
}