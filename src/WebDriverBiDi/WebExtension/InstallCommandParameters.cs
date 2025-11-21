// <copyright file="InstallCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.WebExtension;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the webExtension.install command.
/// </summary>
public class InstallCommandParameters : CommandParameters<InstallCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstallCommandParameters"/> class.
    /// </summary>
    /// <param name="extension">The <see cref="ExtensionData"/> object describing the extension to install.</param>
    public InstallCommandParameters(ExtensionData extension)
    {
        this.ExtensionData = extension;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "webExtension.install";

    /// <summary>
    /// Gets or sets the data of the web extension to install.
    /// </summary>
    [JsonPropertyName("extensionData")]
    public ExtensionData ExtensionData { get; set; }
}
