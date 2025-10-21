// <copyright file="SetDownloadBehaviorCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browser.setDownloadBehavior command.
/// </summary>
public class SetDownloadBehaviorCommandParameters : CommandParameters<SetDownloadBehaviorCommandResult>
{
    private DownloadBehavior? downloadBehavior;
    private List<string>? userContexts;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetDownloadBehaviorCommandParameters"/> class.
    /// </summary>
    public SetDownloadBehaviorCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browser.setDownloadBehavior";

    /// <summary>
    /// Gets or sets the download behavior for the browser.
    /// Setting the value to <see langword="null"/> resets the download behavior to the default.
    /// </summary>
    [JsonPropertyName("downloadBehavior")]
    [JsonInclude]
    public DownloadBehavior? DownloadBehavior { get => this.downloadBehavior; set => this.downloadBehavior = value; }

    /// <summary>
    /// Gets or sets the list of user contexts for which to set the download behavior.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get => this.userContexts; set => this.userContexts = value; }
}
