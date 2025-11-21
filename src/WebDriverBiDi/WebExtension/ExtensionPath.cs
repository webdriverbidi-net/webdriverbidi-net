// <copyright file="ExtensionPath.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.WebExtension;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a browser extension as a full path to a directory containing the extension files.
/// </summary>
public class ExtensionPath : ExtensionData
{
    private readonly string extensionType = "path";

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionPath"/> class.
    /// </summary>
    public ExtensionPath()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionPath"/> class.
    /// </summary>
    /// <param name="path">The full path to a directory containing the extension files.</param>
    public ExtensionPath(string path)
    {
        this.Path = path;
    }

    /// <summary>
    /// Gets the type of extension data this item represents.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => this.extensionType;

    /// <summary>
    /// Gets or sets the full path to a directory containing the extension files.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
}
