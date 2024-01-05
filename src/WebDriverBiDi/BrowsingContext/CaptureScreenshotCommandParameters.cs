// <copyright file="CaptureScreenshotCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.captureScreenshot command.
/// </summary>
public class CaptureScreenshotCommandParameters : CommandParameters<CaptureScreenshotCommandResult>
{
    private string browsingContextId;
    private ImageFormat? format;
    private ClipRectangle? clip;
    private ScreenshotOrigin? origin;

    /// <summary>
    /// Initializes a new instance of the <see cref="CaptureScreenshotCommandParameters" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to capture the screenshot.</param>
    public CaptureScreenshotCommandParameters(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.captureScreenshot";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to capture the screenshot.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the format of the screenshot image.
    /// </summary>
    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImageFormat? Format { get => this.format; set => this.format = value; }

    /// <summary>
    /// Gets or sets the clip rectangle for the screenshot, if any.
    /// </summary>
    [JsonPropertyName("clip")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ClipRectangle? Clip { get => this.clip; set => this.clip = value; }

    /// <summary>
    /// Gets or sets the origin of the clip rectangle for the screenshot, if any.
    /// </summary>
    [JsonPropertyName("origin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ScreenshotOrigin? Origin { get => this.origin; set => this.origin = value; }
}
