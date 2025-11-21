// <copyright file="DisownCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the script.disown command.
/// </summary>
public class DisownCommandParameters : CommandParameters<DisownCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisownCommandParameters"/> class.
    /// </summary>
    /// <param name="target">The script target containing handles to disown.</param>
    /// <param name="handleValues">The handles to disown.</param>
    public DisownCommandParameters(Target target, params string[] handleValues)
    {
        this.Target = target;
        this.Handles.AddRange(handleValues);
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "script.disown";

    /// <summary>
    /// Gets or sets the target for which to disown handles.
    /// </summary>
    [JsonPropertyName("target")]
    public Target Target { get; set; }

    /// <summary>
    /// Gets or sets the list of handles to disown.
    /// </summary>
    [JsonPropertyName("handles")]
    public List<string> Handles { get; set; } = [];
}
