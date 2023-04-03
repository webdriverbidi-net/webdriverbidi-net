// <copyright file="SourceActions.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Input;

using Newtonsoft.Json;

/// <summary>
/// Base class for input actions.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public abstract class SourceActions
{
    /// <summary>
    /// Gets the type of the source actions.
    /// </summary>
    [JsonProperty("type")]
    public abstract string Type { get; }
}