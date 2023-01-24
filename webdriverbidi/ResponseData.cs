// <copyright file="ResponseData.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Data received from a response.
/// </summary>
public class ResponseData
{
    private Dictionary<string, object?> additionalData = new();

    /// <summary>
    /// Gets a value indicating whether the response data is an error.
    /// </summary>
    public virtual bool IsError => false;

    /// <summary>
    /// Gets additional data received in the response.
    /// </summary>
    public Dictionary<string, object?> AdditionalData { get => this.additionalData; internal set => this.additionalData = value; }
}