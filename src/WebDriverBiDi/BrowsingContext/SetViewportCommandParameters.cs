// <copyright file="SetViewportCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides parameters for the browsingContext.create command.
/// </summary>
public class SetViewportCommandParameters : CommandParameters<SetViewportCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetViewportCommandParameters"/> class.
    /// </summary>
    public SetViewportCommandParameters()
    {
    }

    /// <summary>
    /// Gets a value indicating that the viewport should be reset to its default dimensions.
    /// </summary>
    public static Viewport ResetToDefaultViewport => Viewport.ResetToDefaultViewport;

    /// <summary>
    /// Gets a value indicating that the device pixel ratio should be reset to its default value.
    /// </summary>
    public static double ResetToDefaultDevicePixelRatio => -1;

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.setViewport";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to set the viewport.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the viewport dimensions to set. A null value sets the viewport to the default dimensions.
    /// </summary>
    [JsonPropertyName("viewport")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(ConditionalNullPropertyJsonConverter<Viewport>))]
    public Viewport? Viewport { get; set; }

    /// <summary>
    /// Gets or sets the device pixel ratio of the viewport.
    /// </summary>
    [JsonPropertyName("devicePixelRatio")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(ConditionalNullPropertyJsonConverter<double>))]
    public double? DevicePixelRatio { get; set; }

    /// <summary>
    /// Gets or sets the user context IDs for which to set the viewport.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContextIds { get; set; }
}
