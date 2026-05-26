// <copyright file="StartScreencastCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.startScreencast command.
/// </summary>
public class StartScreencastCommandParameters : CommandParameters<StartScreencastCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartScreencastCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to start the screencast.</param>
    public StartScreencastCommandParameters(string browsingContextId)
    {
        this.BrowsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.startScreencast";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to create the screencast.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the screencast file.
    /// </summary>
    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the video constraints for a screencast.
    /// </summary>
    [JsonPropertyName("video")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MediaTrackConstraints? Video { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the screencast should include an audio track. If omitted, defaults to false.
    /// </summary>
    [JsonPropertyName("audio")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Audio { get; set; }
}
