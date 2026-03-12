// <copyright file="SetViewportCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides parameters for the browsingContext.setViewport command.
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
    /// Gets a sentinel value for the <see cref="Viewport"/> property that signals the remote
    /// end to reset the viewport to its default dimensions. Assign this to the
    /// <see cref="Viewport"/> property; do not use it as a command-level reset.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a <em>property-level sentinel</em>, not a command-level reset. It must be
    /// assigned to the <see cref="Viewport"/> property of a
    /// <see cref="SetViewportCommandParameters"/> instance:
    /// </para>
    /// <code>
    /// await driver.BrowsingContext.SetViewportAsync(
    ///     new SetViewportCommandParameters
    ///     {
    ///         Viewport = SetViewportCommandParameters.ResetToDefaultViewport
    ///     });
    /// </code>
    /// <para>
    /// When serialized, the sentinel causes the <c>viewport</c> field to be written as
    /// JSON <c>null</c>, instructing the remote end to restore the default viewport.
    /// Assigning a C# <see langword="null"/> to <see cref="Viewport"/> omits the field
    /// entirely, leaving the current viewport unchanged.
    /// </para>
    /// </remarks>
    public static Viewport ResetToDefaultViewport => Viewport.ResetToDefaultViewport;

    /// <summary>
    /// Gets a sentinel value for the <see cref="DevicePixelRatio"/> property that signals
    /// the remote end to reset the device pixel ratio to its default value. Assign this to
    /// the <see cref="DevicePixelRatio"/> property; do not use it as a command-level reset.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a <em>property-level sentinel</em>, not a command-level reset. It must be
    /// assigned to the <see cref="DevicePixelRatio"/> property of a
    /// <see cref="SetViewportCommandParameters"/> instance:
    /// </para>
    /// <code>
    /// await driver.BrowsingContext.SetViewportAsync(
    ///     new SetViewportCommandParameters
    ///     {
    ///         DevicePixelRatio = SetViewportCommandParameters.ResetToDefaultDevicePixelRatio
    ///     });
    /// </code>
    /// <para>
    /// When serialized, the sentinel (any negative value) causes the
    /// <c>devicePixelRatio</c> field to be written as JSON <c>null</c>, instructing the
    /// remote end to restore the default pixel ratio. Assigning a C# <see langword="null"/>
    /// to <see cref="DevicePixelRatio"/> omits the field entirely, leaving the current
    /// pixel ratio unchanged.
    /// </para>
    /// </remarks>
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
    [JsonConverter(typeof(SentinelNullJsonConverter<Viewport, ViewportSentinelChecker>))]
    public Viewport? Viewport { get; set; }

    /// <summary>
    /// Gets or sets the device pixel ratio of the viewport.
    /// </summary>
    [JsonPropertyName("devicePixelRatio")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(SentinelNullJsonConverter<double, NegativeDoubleSentinelChecker>))]
    public double? DevicePixelRatio { get; set; }

    /// <summary>
    /// Gets or sets the user context IDs for which to set the viewport.
    /// </summary>
    /// <remarks>
    /// This property is nullable to distinguish between omitting the property from the JSON payload (null)
    /// and sending an empty array (empty list). When null, the property is not included in the command;
    /// when an empty list, an empty array is sent to the remote end.
    /// </remarks>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get; set; }
}
