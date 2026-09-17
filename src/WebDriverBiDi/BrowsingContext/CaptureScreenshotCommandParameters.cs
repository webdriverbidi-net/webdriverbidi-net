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
    /// <summary>
    /// Initializes a new instance of the <see cref="CaptureScreenshotCommandParameters" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to capture the screenshot.</param>
    public CaptureScreenshotCommandParameters(string browsingContextId)
    {
        this.BrowsingContextId = browsingContextId;
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
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the clip rectangle for the screenshot, if any.
    /// </summary>
    [JsonPropertyName("clip")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ClipRectangle? Clip { get; set; }

    /// <summary>
    /// Gets or sets the format of the screenshot image.
    /// </summary>
    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImageFormat? Format { get; set; }

    /// <summary>
    /// Gets or sets the size of the screenshot image.
    /// </summary>
    [JsonPropertyName("imageSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ImageSize? ImageSize { get; set; }

    /// <summary>
    /// Gets or sets the area the screenshot captures. Defaults to
    /// <see cref="ScreenshotOrigin.Viewport"/> when omitted.
    /// </summary>
    /// <remarks>
    /// The area is the visual viewport, or the whole document, which yields a full-page screenshot.
    /// The image is the part of a <see cref="Clip"/> that falls within the chosen area, so this value
    /// applies whether or not a clip is supplied. A <see cref="BoxClipRectangle"/> is positioned relative
    /// to the chosen area. An <see cref="ElementClipRectangle"/> is positioned at its element whatever the
    /// origin, so with the viewport origin an element scrolled out of view leaves nothing to capture.
    /// </remarks>
    [JsonPropertyName("origin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ScreenshotOrigin? Origin { get; set; }
}
