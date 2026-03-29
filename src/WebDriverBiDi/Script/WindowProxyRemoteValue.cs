// <copyright file="WindowProxyRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a window proxy representing the JavaScript window object.
/// It also provides the ability to convert to a local value for use as an argument for
/// script execution on the remote end.
/// </summary>
public record WindowProxyRemoteValue : ObjectReferenceRemoteValue, ITypeSafeRemoteValue<WindowProxyProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowProxyRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal WindowProxyRemoteValue()
        : base()
    {
        this.Type = RemoteValueType.Window;
        this.Value = default;
    }

    /// <summary>
    /// Gets the properties of the proxy object representing the window.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonInclude]
    [JsonRequired]
    public WindowProxyProperties Value { get; internal set; }
}
