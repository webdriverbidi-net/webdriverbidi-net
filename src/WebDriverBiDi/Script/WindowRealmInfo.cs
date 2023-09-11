// <copyright file="WindowRealmInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object representing a window realm for executing script.
/// </summary>
public class WindowRealmInfo : RealmInfo
{
    private string browsingContextId = string.Empty;
    private string? sandbox;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowRealmInfo"/> class.
    /// </summary>
    internal WindowRealmInfo()
        : base()
    {
    }

    /// <summary>
    /// Gets the ID of the browsing context containing this window realm.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    public string BrowsingContext { get => this.browsingContextId; internal set => this.browsingContextId = value; }

    /// <summary>
    /// Gets the sandbox name for the realm.
    /// </summary>
    [JsonPropertyName("sandbox")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sandbox { get => this.sandbox; internal set => this.sandbox = value; }
} 