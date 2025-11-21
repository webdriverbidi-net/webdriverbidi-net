// <copyright file="SetFilesCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// Provides parameters for the input.setFiles command.
/// </summary>
public class SetFilesCommandParameters : CommandParameters<SetFilesCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetFilesCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The browsing context ID containing the element for which to set the files.</param>
    /// <param name="element">The element for which to set the file list. Must be of type {input type="file"}.</param>
    public SetFilesCommandParameters(string browsingContextId, SharedReference element)
    {
        this.BrowsingContextId = browsingContextId;
        this.Element = element;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "input.setFiles";

    /// <summary>
    /// Gets or sets the browsing context ID containing the element for which to set the files.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the element for which to set the file list. Note that the element must be of type {input type="file"}.
    /// </summary>
    [JsonPropertyName("element")]
    public SharedReference Element { get; set; }

    /// <summary>
    /// Gets the list of files to be set on the element.
    /// </summary>
    [JsonPropertyName("files")]
    public List<string> Files { get; } = [];
}
