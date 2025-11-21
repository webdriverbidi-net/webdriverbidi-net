// <copyright file="CommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Data received from a response.
/// </summary>
public record CommandResult
{
    /// <summary>
    /// Gets a value indicating whether the response data is an error.
    /// </summary>
    public virtual bool IsError => false;

    /// <summary>
    /// Gets additional data received in the response.
    /// </summary>
    public ReceivedDataDictionary AdditionalData { get; internal set; } = ReceivedDataDictionary.EmptyDictionary;
}
