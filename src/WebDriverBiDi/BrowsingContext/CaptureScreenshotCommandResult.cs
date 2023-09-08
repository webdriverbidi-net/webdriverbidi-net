// <copyright file="CaptureScreenshotCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Result for capturing a screenshot using the browserContext.captureScreenshot command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CaptureScreenshotCommandResult : CommandResult
{
    private string base64Screenshot = string.Empty;

    [JsonConstructor]
    private CaptureScreenshotCommandResult()
    {
    }

    /// <summary>
    /// Gets the screenshot image data as a base64-encoded string.
    /// </summary>
    [JsonProperty("data")]
    [JsonRequired]
    public string Data { get => this.base64Screenshot; internal set => this.base64Screenshot = value; }
}
