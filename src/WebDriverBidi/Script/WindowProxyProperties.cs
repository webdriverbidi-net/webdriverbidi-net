// <copyright file="WindowProxyProperties.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object representing the properties of a window proxy object.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class WindowProxyProperties
{
    private string context = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowProxyProperties"/> class.
    /// </summary>
    internal WindowProxyProperties()
    {
    }

    /// <summary>
    /// Gets the browsing context ID for the window proxy.
    /// </summary>
    [JsonProperty("context")]
    [JsonRequired]
    public string Context { get => this.context; internal set => this.context = value; }
}
