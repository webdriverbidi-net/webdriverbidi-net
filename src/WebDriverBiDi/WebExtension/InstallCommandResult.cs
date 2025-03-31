// <copyright file="InstallCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.WebExtension;

using System.Text.Json.Serialization;

/// <summary>
/// Result for installing a web extension using the webExtension.install command.
/// </summary>
public record InstallCommandResult : CommandResult
{
    private string extensionId = string.Empty;

    [JsonConstructor]
    private InstallCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the installed extension as specified in the extension manifest.
    /// </summary>
    [JsonPropertyName("extension")]
    [JsonRequired]
    [JsonInclude]
    public string ExtensionId { get => this.extensionId; private set => this.extensionId = value; }
}
