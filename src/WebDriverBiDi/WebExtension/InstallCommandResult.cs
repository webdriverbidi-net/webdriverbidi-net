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
    /// <summary>
    /// Initializes a new instance of the <see cref="InstallCommandResult"/> class.
    /// </summary>
    [JsonConstructor]
    internal InstallCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID the remote end assigned to the installed extension. Pass this value to
    /// <see cref="UninstallCommandParameters"/> to remove the extension.
    /// </summary>
    [JsonPropertyName("extension")]
    [JsonRequired]
    [JsonInclude]
    public string ExtensionId { get; internal set; } = string.Empty;
}
