// <copyright file="WebDriverBiDiEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Base EventArgs class for use with WebDriver Bidi events.
/// </summary>
public record WebDriverBiDiEventArgs
{
    /// <summary>
    /// Gets the extension properties received inside the <c>params</c> object of the event:
    /// properties beside the event's specified members that the event args type does not define.
    /// </summary>
    /// <remarks>
    /// Properties found on the event envelope instead are exposed through <see cref="AdditionalEventProperties"/>.
    /// </remarks>
    public ReceivedDataDictionary AdditionalData { get; internal set; } = ReceivedDataDictionary.EmptyDictionary;

    /// <summary>
    /// Gets the extension properties received on the event envelope: properties beside
    /// <c>type</c>, <c>method</c> and <c>params</c>, such as Chromium's <c>goog:channel</c>.
    /// </summary>
    public ReceivedDataDictionary AdditionalEventProperties { get; internal set; } = ReceivedDataDictionary.EmptyDictionary;
}
