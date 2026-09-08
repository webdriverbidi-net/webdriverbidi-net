// <copyright file="UserPromptHandlerType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The ways a user prompt handler can respond to a prompt. The kind of prompt a handler applies to is
/// chosen by which <see cref="UserPromptHandler"/> property the value is assigned to.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<UserPromptHandlerType>))]
public enum UserPromptHandlerType
{
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
