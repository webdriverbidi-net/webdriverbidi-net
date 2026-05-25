// <copyright file="StopScreencastCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.stopScreencast command.
/// </summary>
public class StopScreencastCommandParameters : CommandParameters<StopScreencastCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StopScreencastCommandParameters"/> class.
    /// </summary>
    /// <param name="screencastId">The ID of the screencast to stop.</param>
    public StopScreencastCommandParameters(string screencastId)
    {
        this.ScreencastId = screencastId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.stopScreencast";

    /// <summary>
    /// Gets or sets the ID of the screencast to stop.
    /// </summary>
    [JsonPropertyName("screencast")]
    [JsonInclude]
    [JsonRequired]
    public string ScreencastId { get; set; }
}
