// <copyright file="AddPreloadScriptCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the script.addPreloadScript command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class AddPreloadScriptCommandSettings : CommandSettings
{
    private string expression;
    private string? sandbox;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPreloadScriptCommandSettings"/> class.
    /// </summary>
    /// <param name="expression">The expression containing the preload script.</param>
    public AddPreloadScriptCommandSettings(string expression)
    {
        this.expression = expression;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "script.addPreloadScript";

    /// <summary>
    /// Gets the type of the result of the command.
    /// </summary>
    public override Type ResultType => typeof(AddPreloadScriptCommandResult);

    /// <summary>
    /// Gets or sets the expression defining the preload script.
    /// </summary>
    [JsonProperty("expression")]
    public string Expression { get => this.expression; set => this.expression = value; }

    /// <summary>
    /// Gets or sets the sandbox name of the preload script.
    /// </summary>
    [JsonProperty("sandbox", NullValueHandling = NullValueHandling.Ignore)]
    public string? Sandbox { get => this.sandbox; set => this.sandbox = value; }
}
