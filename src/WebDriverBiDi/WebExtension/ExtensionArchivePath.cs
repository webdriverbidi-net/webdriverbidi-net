// <copyright file="ExtensionArchivePath.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.WebExtension;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a browser extension packaged inside a zip archive.
/// </summary>
public class ExtensionArchivePath : ExtensionData
{
    private readonly string extensionType = "archivePath";

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionArchivePath"/> class.
    /// </summary>
    public ExtensionArchivePath()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionArchivePath"/> class.
    /// </summary>
    /// <param name="path">The full path and file name to the zip archive file containing the extension.</param>
    public ExtensionArchivePath(string path)
    {
        this.Path = path;
    }

    /// <summary>
    /// Gets the type of extension data this item represents.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => this.extensionType;

    /// <summary>
    /// Gets or sets the full path and file name to the zip archive file containing the extension.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
}
