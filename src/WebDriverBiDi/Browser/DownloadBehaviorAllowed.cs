// <copyright file="DownloadBehaviorAllowed.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text.Json.Serialization;

/// <summary>
/// Class describing the settings for allowing download in the browser.
/// </summary>
public class DownloadBehaviorAllowed : DownloadBehavior
{
    private string? destinationFolder;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadBehaviorAllowed"/> class.
    /// </summary>
    public DownloadBehaviorAllowed()
        : base(DownloadBehaviorType.Allowed)
    {
    }

    /// <summary>
    /// Gets or sets the destination folder for downloaded files.
    /// </summary>
    [JsonPropertyName("destinationFolder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DestinationFolder { get => this.destinationFolder; set => this.destinationFolder = value; }
}
