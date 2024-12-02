// <copyright file="ExtensionBase64Encoded.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.WebExtension;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a browser extension packaged inside a zip archive encoded as a base64-encoded string.
/// </summary>
public class ExtensionBase64Encoded : ExtensionData
{
    private readonly string extensionType = "base64";
    private string extensionValue = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionBase64Encoded"/> class.
    /// </summary>
    public ExtensionBase64Encoded()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionBase64Encoded"/> class.
    /// </summary>
    /// <param name="extensionValue">A web extension zip archive represented as a base64-encoded string.</param>
    public ExtensionBase64Encoded(string extensionValue)
    {
        this.extensionValue = extensionValue;
    }

    /// <summary>
    /// Gets the type of extension data this item represents.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => this.extensionType;

    /// <summary>
    /// Gets or sets a web extension zip archive represented as a base64-encoded string..
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get => this.extensionValue; set => this.extensionValue = value; }
}
