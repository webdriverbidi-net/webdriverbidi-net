// <copyright file="WebDriverBidiEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Base EventArgs class for use with WebDriver Bidi events.
/// </summary>
public class WebDriverBidiEventArgs : EventArgs
{
    private ReceivedDataDictionary additionalData = ReceivedDataDictionary.EmptyDictionary;

    /// <summary>
    /// Gets or sets additional extended data sent with the event.
    /// </summary>
    public ReceivedDataDictionary AdditionalData { get => this.additionalData; set => this.additionalData = value; }
}