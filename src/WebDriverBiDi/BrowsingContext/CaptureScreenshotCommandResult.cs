// <copyright file="CaptureScreenshotCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Result for capturing a screenshot using the browserContext.captureScreenshot command.
/// </summary>
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
    [JsonPropertyName("data")]
    [JsonRequired]
    [JsonInclude]
    public string Data { get => this.base64Screenshot; private set => this.base64Screenshot = value; }
}
