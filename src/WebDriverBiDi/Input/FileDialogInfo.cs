// <copyright file="FileDialogInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// Object containing event data for the input.fileDialogOpened event.
/// </summary>
public record FileDialogInfo
{
    private string browsingContextId = string.Empty;
    private RemoteValue? elementValue;
    private SharedReference? element;
    private bool multiple;

    [JsonConstructor]
    private FileDialogInfo()
    {
    }

    /// <summary>
    /// Gets the ID of the browsing context for which the user prompt was opened.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get => this.browsingContextId; private set => this.browsingContextId = value; }

    /// <summary>
    /// Gets a value indicating whether the file dialog supports multiple file names.
    /// </summary>
    [JsonPropertyName("multiple")]
    [JsonRequired]
    [JsonInclude]
    public bool Multiple { get => this.multiple; private set => this.multiple = value; }

    /// <summary>
    /// Gets the reference to the element that invoked the file dialog, if present.
    /// </summary>
    [JsonIgnore]
    public SharedReference? Element => this.element;

    /// <summary>
    /// Sets a reference to the element that invoked the file dialog for serialization purposes.
    /// </summary>
    [JsonPropertyName("element")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal RemoteValue? SerializableElement
    {
        set
        {
            if (value is not null)
            {
                this.elementValue = value;
                this.element = value.ToSharedReference();
            }
        }
    }
}