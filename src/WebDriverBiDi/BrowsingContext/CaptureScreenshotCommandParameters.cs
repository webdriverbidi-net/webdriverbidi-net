// <copyright file="CaptureScreenshotCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.captureScreenshot command.
/// </summary>
public class CaptureScreenshotCommandParameters : CommandParameters<CaptureScreenshotCommandResult>
{
    private string browsingContextId;
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
    [JsonIgnore]
    public override string MethodName => "browsingContext.captureScreenshot";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to capture the screenshot.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the clip rectangle for the screenshot, if any.
    /// </summary>
    [JsonPropertyName("clip")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ClipRectangle? Clip { get => this.clip; set => this.clip = value; }
}