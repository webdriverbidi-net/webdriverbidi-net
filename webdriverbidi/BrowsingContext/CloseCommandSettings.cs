// <copyright file="CloseCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using System;
using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.close command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CloseCommandSettings : CommandSettings
{
    private string browsingContextId;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloseCommandSettings" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context to close.</param>
    public CloseCommandSettings(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "browsingContext.close";

    /// <summary>
    /// Gets the type of the result of the command.
    /// </summary>
    public override Type ResultType => typeof(EmptyResult);

    /// <summary>
    /// Gets or sets the ID of the browsing context to close.
    /// </summary>
    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }
}