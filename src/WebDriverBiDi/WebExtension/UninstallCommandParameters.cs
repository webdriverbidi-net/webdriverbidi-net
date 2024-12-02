// <copyright file="UninstallCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.WebExtension;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the webExtension.uninstall command.
/// </summary>
public class UninstallCommandParameters : CommandParameters<EmptyResult>
{
    private string extensionId;

    /// <summary>
    /// Initializes a new instance of the <see cref="UninstallCommandParameters"/> class.
    /// </summary>
    /// <param name="extensionId">The ID of the extension to uninstall.</param>
    public UninstallCommandParameters(string extensionId)
    {
        this.extensionId = extensionId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "webExtension.uninstall";

    /// <summary>
    /// Gets or sets the data of the web extension to install.
    /// </summary>
    [JsonPropertyName("extension")]
    public string ExtensionId { get => this.extensionId; set => this.extensionId = value; }
}
