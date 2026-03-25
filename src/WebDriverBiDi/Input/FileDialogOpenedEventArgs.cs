// <copyright file="FileDialogOpenedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// Object containing event data for the input.fileDialogOpened event.
/// </summary>
public record FileDialogOpenedEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileDialogOpenedEventArgs"/> class.
    /// </summary>
    [JsonConstructor]
    public FileDialogOpenedEventArgs()
    {
    }

    /// <summary>
    /// Gets the ID of the browsing context for which the user prompt was opened.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the file dialog supports multiple file names.
    /// </summary>
    [JsonPropertyName("multiple")]
    [JsonRequired]
    [JsonInclude]
    public bool IsMultiple { get; internal set; }

    /// <summary>
    /// Gets the ID of the user context for which the user prompt was opened.
    /// </summary>
    [JsonPropertyName("userContext")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? UserContextId { get; internal set; }

    /// <summary>
    /// Gets the reference to the element that invoked the file dialog, if present.
    /// </summary>
    [JsonPropertyName("element")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SharedReference? Element { get; internal set; }
}
