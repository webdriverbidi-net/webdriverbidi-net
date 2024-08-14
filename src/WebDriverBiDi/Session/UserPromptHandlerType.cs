// <copyright file="UserPromptHandlerType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The types of user prompts.
/// </summary>
[JsonConverterWithArgs(typeof(EnumValueJsonConverter<UserPromptHandlerType>), Unknown)]
public enum UserPromptHandlerType
{
    /// <summary>
    /// The handler type is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// Handler accepts the user prompt.
    /// </summary>
    Accept,

    /// <summary>
    /// Handler dismisses the user prompt.
    /// </summary>
    Dismiss,

    /// <summary>
    /// Handler ignores the user prompt.
    /// </summary>
    Ignore,
}
