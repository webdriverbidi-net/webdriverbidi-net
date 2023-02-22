// <copyright file="PrintCommandResult.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Result for getting the tree of browsing contexts using the browserContext.getTree command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PrintCommandResult : CommandResult
{
    private string base64PdfPrintOutput = string.Empty;

    [JsonConstructor]
    private PrintCommandResult()
    {
    }

    /// <summary>
    /// Gets the screenshot image data as a base64-encoded string.
    /// </summary>
    [JsonProperty("data")]
    [JsonRequired]
    public string Data { get => this.base64PdfPrintOutput; internal set => this.base64PdfPrintOutput = value; }
}