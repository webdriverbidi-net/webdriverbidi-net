// <copyright file="CaptureScreenshotCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System;
using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.captureScreenshot command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CaptureScreenshotCommandParameters : CommandParameters<CaptureScreenshotCommandResult>
{
    private string browsingContextId;
    private ImageFormat? format;
    private ClipRectangle? clip;

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
    public override string MethodName => "browsingContext.captureScreenshot";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to capture the screenshot.
    /// </summary>
    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the format of the screenshot image.
    /// </summary>
    [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
    public ImageFormat? Format { get => this.format; set => this.format = value; }

    /// <summary>
    /// Gets or sets the clip rectangle for the screenshot, if any.
    /// </summary>
    [JsonProperty("clip", NullValueHandling = NullValueHandling.Ignore)]
    public ClipRectangle? Clip { get => this.clip; set => this.clip = value; }
}
