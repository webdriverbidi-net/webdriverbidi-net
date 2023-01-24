// <copyright file="CaptureScreenshotCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using System;
using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.captureScreenshot command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CaptureScreenshotCommandSettings : CommandData<CaptureScreenshotCommandResult>
{
    private string browsingContextId;

    /// <summary>
    /// Initializes a new instance of the <see cref="CaptureScreenshotCommandSettings" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to capture the screenshot.</param>
    public CaptureScreenshotCommandSettings(string browsingContextId)
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
}