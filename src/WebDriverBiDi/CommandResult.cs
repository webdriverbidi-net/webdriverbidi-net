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
    /// Gets the extension properties received inside the <c>result</c> object of the response:
    /// properties beside the result's specified members that the result type does not define.
    /// </summary>
    /// <remarks>
    /// This is the receiving-side counterpart of <see cref="CommandParameters.AdditionalData"/>, which
    /// places properties inside the <c>params</c> object of a command. Properties found on the
    /// response envelope instead are exposed through <see cref="AdditionalResponseProperties"/>.
    /// </remarks>
    public ReceivedDataDictionary AdditionalData { get; internal set; } = ReceivedDataDictionary.EmptyDictionary;

    /// <summary>
    /// Gets the extension properties received on the response envelope: properties beside
    /// <c>type</c>, <c>id</c> and <c>result</c>, such as Chromium's <c>goog:channel</c>.
    /// </summary>
    /// <remarks>
    /// This is the receiving-side counterpart of <see cref="Protocol.Command.AdditionalCommandProperties"/>.
    /// </remarks>
    public ReceivedDataDictionary AdditionalResponseProperties { get; internal set; } = ReceivedDataDictionary.EmptyDictionary;
}
