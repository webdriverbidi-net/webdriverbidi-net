// <copyright file="UserPromptType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The types of user prompts.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<UserPromptType>))]
public enum UserPromptType
{
    /// <summary>
    /// An alert displayed for user notification.
    /// </summary>
    Alert,

    /// <summary>
    /// A confirmation asking the user to choose yes or no.
    /// </summary>
    Confirm,

    /// <summary>
    /// A prompt asking the user to provide textual information.
    /// </summary>
    Prompt,

    /// <summary>
    /// A prompt informing the user that the operation will unload the current page.
    /// </summary>
    BeforeUnload,
}